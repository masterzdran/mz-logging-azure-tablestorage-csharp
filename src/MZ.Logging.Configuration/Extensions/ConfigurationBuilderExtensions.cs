using Microsoft.Extensions.Configuration;

namespace MZ.Logging.Configuration;

/// <summary>
/// Extension methods for configuring Azure logging providers.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds Azure Key Vault configuration source.
    /// </summary>
    /// <param name="configurationBuilder">The configuration builder.</param>
    /// <param name="keyVaultUri">The Key Vault URI.</param>
    /// <returns>The configuration builder for chaining.</returns>
    public static IConfigurationBuilder AddAzureKeyVault(
        this IConfigurationBuilder configurationBuilder,
        string keyVaultUri)
    {
        if (configurationBuilder == null)
            throw new ArgumentNullException(nameof(configurationBuilder));

        if (string.IsNullOrWhiteSpace(keyVaultUri))
            throw new ArgumentException("Key Vault URI cannot be empty.", nameof(keyVaultUri));

        return configurationBuilder.Add(new AzureKeyVaultConfigurationSource(keyVaultUri));
    }

    /// <summary>
    /// Adds Azure App Configuration source.
    /// </summary>
    /// <param name="configurationBuilder">The configuration builder.</param>
    /// <param name="connectionString">The App Configuration connection string.</param>
    /// <returns>The configuration builder for chaining.</returns>
    public static IConfigurationBuilder AddAzureAppConfiguration(
        this IConfigurationBuilder configurationBuilder,
        string connectionString)
    {
        if (configurationBuilder == null)
            throw new ArgumentNullException(nameof(configurationBuilder));

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty.", nameof(connectionString));

        return configurationBuilder.Add(new AzureAppConfigurationSource(connectionString));
    }

    /// <summary>
    /// Loads Azure Table Storage logging configuration.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The parsed configuration.</returns>
    public static AzureTableStorageLoggingConfiguration GetAzureTableStorageLoggingConfiguration(
        this IConfiguration configuration,
        string sectionName = "AzureTableStorageLogging")
    {
        var config = new AzureTableStorageLoggingConfiguration();
        configuration.GetSection(sectionName).Bind(config);

        var validationErrors = config.Validate();
        if (validationErrors.Any())
            throw new InvalidOperationException($"Invalid logging configuration: {string.Join(", ", validationErrors)}");

        return config;
    }
}

/// <summary>
/// Configuration source for Azure Key Vault.
/// </summary>
internal class AzureKeyVaultConfigurationSource : IConfigurationSource
{
    private readonly string _keyVaultUri;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureKeyVaultConfigurationSource"/> class.
    /// </summary>
    /// <param name="keyVaultUri">The Key Vault URI.</param>
    public AzureKeyVaultConfigurationSource(string keyVaultUri)
    {
        _keyVaultUri = keyVaultUri;
    }

    /// <summary>
    /// Builds the configuration provider.
    /// </summary>
    /// <param name="builder">The configuration builder.</param>
    /// <returns>The configuration provider.</returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new AzureKeyVaultConfigurationProvider(_keyVaultUri);
    }
}

/// <summary>
/// Configuration source for Azure App Configuration.
/// </summary>
internal class AzureAppConfigurationSource : IConfigurationSource
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureAppConfigurationSource"/> class.
    /// </summary>
    /// <param name="connectionString">The App Configuration connection string.</param>
    public AzureAppConfigurationSource(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Builds the configuration provider.
    /// </summary>
    /// <param name="builder">The configuration builder.</param>
    /// <returns>The configuration provider.</returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new AzureAppConfigurationProvider(_connectionString);
    }
}
