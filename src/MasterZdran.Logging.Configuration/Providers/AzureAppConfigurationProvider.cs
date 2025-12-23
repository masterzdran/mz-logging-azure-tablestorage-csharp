using Azure.Data.AppConfiguration;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using MasterZdran.Logging.AzureTableStorage;

namespace MasterZdran.Logging.Configuration;

/// <summary>
/// Configuration provider for Azure App Configuration service.
/// Supports feature flags, key-value pairs, and secret references.
/// </summary>
public class AzureAppConfigurationProvider : IConfigurationProvider
{
    private readonly string _connectionString;
    private ConfigurationClient? _client;
    private readonly Dictionary<string, string> _configuration = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureAppConfigurationProvider"/> class.
    /// </summary>
    /// <param name="connectionString">The App Configuration connection string.</param>
    /// <exception cref="ArgumentException">Thrown when connection string is invalid.</exception>
    public AzureAppConfigurationProvider(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty.", nameof(connectionString));

        _connectionString = connectionString;

        try
        {
            _client = new ConfigurationClient(_connectionString);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize App Configuration client.", ex);
        }
    }

    /// <summary>
    /// Loads configuration from App Configuration.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task LoadAsync()
    {
        if (_client == null)
            return;

        try
        {
            var settings = _client.GetConfigurationSettingsAsync(
                new SettingSelector { LabelFilter = "*" });

            await foreach (var setting in settings)
            {
                if (setting.ContentType?.StartsWith("application/vnd.microsoft.appconfig.keyvault") == true)
                {
                    // Handle Key Vault reference
                    try
                    {
                        var secretClient = new Azure.Security.KeyVault.Secrets.SecretClient(
                            new Uri(setting.Value.Split('/')[2]),
                            new DefaultAzureCredential());
                        var secretName = setting.Value.Split('/').Last().Replace("}", "");
                        var secret = await secretClient.GetSecretAsync(secretName).ConfigureAwait(false);
                        _configuration[NormalizeKey(setting.Key, setting.Label)] = secret.Value.Value;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to load Key Vault reference '{setting.Key}': {ex.Message}");
                    }
                }
                else
                {
                    _configuration[NormalizeKey(setting.Key, setting.Label)] = setting.Value;
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load configuration from App Configuration.", ex);
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

    private static string NormalizeKey(string key, string? label)
    {
        return string.IsNullOrEmpty(label)
            ? key.Replace(":", "--")
            : $"{key.Replace(":", "--")}--{label}";
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
        throw new NotSupportedException("Azure App Configuration provider does not support setting values.");
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
