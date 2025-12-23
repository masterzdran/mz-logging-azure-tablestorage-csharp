using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MZ.Logging.AzureTableStorage;

/// <summary>
/// Extension methods for registering Azure Table Storage logging with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Azure Table Storage logging to the service collection, including Microsoft.Extensions.Logging support.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The Azure Table Storage connection string.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="loggerName">The logger name.</param>
    /// <param name="defaultTraceId">The default trace ID.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddAzureTableStorageLogging(
        this IServiceCollection services,
        string connectionString,
        string tableName,
        string loggerName,
        string defaultTraceId = "default-trace-id")
    {
        // Validate inputs (OWASP: Prevent injection)
        if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));
        if (string.IsNullOrWhiteSpace(loggerName)) throw new ArgumentException("Logger name cannot be null or empty.", nameof(loggerName));

        // Register storage implementation
        services.AddSingleton<ILogStorage>(sp => new Storage.AzureTableStorage(connectionString, tableName));

        // Register logger provider
        services.AddSingleton<Logging.AzureTableStorageLoggerProvider>(sp =>
            new Logging.AzureTableStorageLoggerProvider(sp.GetRequiredService<ILogStorage>(), loggerName, defaultTraceId));

        // Register main logger
        services.AddSingleton(sp =>
            new AzureTableStorageLogger(sp.GetRequiredService<ILogStorage>(), loggerName, defaultTraceId));

        // Add to logging framework
        services.AddLogging(builder =>
            builder.AddProvider(builder.Services.BuildServiceProvider().GetRequiredService<Logging.AzureTableStorageLoggerProvider>()));

        return services;
    }
}
