using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using MZ.Logging.AzureTableStorage;

namespace MZ.Logging.Configuration;

/// <summary>
/// Configuration provider for loading settings from Azure Key Vault.
/// Implements secure secret management with OWASP best practices.
/// </summary>
public class AzureKeyVaultConfigurationProvider : IConfigurationProvider
{
    private readonly string _keyVaultUri;
    private readonly SecretClient? _secretClient;
    private readonly Dictionary<string, string> _configuration = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureKeyVaultConfigurationProvider"/> class.
    /// </summary>
    /// <param name="keyVaultUri">The URI of the Azure Key Vault.</param>
    /// <exception cref="ArgumentException">Thrown when URI is invalid.</exception>
    public AzureKeyVaultConfigurationProvider(string keyVaultUri)
    {
        if (string.IsNullOrWhiteSpace(keyVaultUri))
            throw new ArgumentException("Key Vault URI cannot be empty.", nameof(keyVaultUri));

        _keyVaultUri = keyVaultUri;

        try
        {
            _secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize Key Vault client. Ensure you have proper Azure credentials configured.", ex);
        }
    }

    /// <summary>
    /// Loads configuration from Key Vault.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task LoadAsync()
    {
        if (_secretClient == null)
            return;

        try
        {
            var secretProperties = _secretClient.GetPropertiesOfSecretsAsync();

            await foreach (var secretProperty in secretProperties)
            {
                try
                {
                    var secret = await _secretClient.GetSecretAsync(secretProperty.Name).ConfigureAwait(false);
                    _configuration[NormalizeKeyName(secretProperty.Name)] = secret.Value.Value;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load secret '{secretProperty.Name}': {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load secrets from Key Vault.", ex);
        }
    }

    /// <summary>
    /// Gets a configuration value by key.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <returns>The configuration value or null if not found.</returns>
    public string? GetValue(string key)
    {
        return _configuration.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Attempts to get a configuration value.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="value">The configuration value.</param>
    /// <returns>True if found; otherwise false.</returns>
    public bool TryGetValue(string key, out string? value)
    {
        return _configuration.TryGetValue(key, out value);
    }

    bool IConfigurationProvider.TryGet(string key, out string? value)
    {
        return TryGetValue(key, out value);
    }

    /// <summary>
    /// Gets all configuration keys.
    /// </summary>
    /// <returns>Enumeration of configuration keys.</returns>
    public IEnumerable<string> GetKeys()
    {
        return _configuration.Keys;
    }

    private static string NormalizeKeyName(string keyName)
    {
        // Normalize Key Vault naming (replace hyphens with colons for section hierarchy)
        return keyName.Replace("--", ":");
    }

    void IConfigurationProvider.Load() => LoadAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Sets a configuration value. This operation is not supported by this provider.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="value">The configuration value.</param>
    /// <exception cref="NotSupportedException">This provider does not support setting values.</exception>
    public void Set(string key, string? value)
    {
        throw new NotSupportedException("Azure Key Vault provider does not support setting values.");
    }

    /// <summary>
    /// Gets a reload token for configuration changes. Not implemented for this provider.
    /// </summary>
    /// <returns>An <see cref="IChangeToken"/> that signals when configuration changes.</returns>
    public IChangeToken GetReloadToken()
    {
        return new NullChangeToken();
    }

    /// <summary>
    /// Gets child configuration keys for a given parent path.
    /// </summary>
    /// <param name="earlierKeys">Previously discovered keys.</param>
    /// <param name="parentPath">The parent path to search under.</param>
    /// <returns>An enumerable of child keys.</returns>
    public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
    {
        return earlierKeys ?? Enumerable.Empty<string>();
    }

    private class NullChangeToken : IChangeToken
    {
        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state) => NullDisposable.Instance;
        public bool HasChanged => false;
        public bool ActiveChangeCallbacks => false;

        private class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new();
            public void Dispose() { }
        }
    }
}
