// See https://aka.ms/new-console-app-templates for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MZ.Logging.AzureTableStorage;
using MZ.Logging.AzureTableStorage.Logging.MZ.Logging.AzureTableStorage;


// Load configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

// Get settings from configuration
var connectionString = configuration["AzureTableStorage:ConnectionString"]
    ?? throw new InvalidOperationException("AzureTableStorage:ConnectionString is not configured");
var tableName = configuration["AzureTableStorage:TableName"] ?? "logs";
var loggerName = configuration["AzureTableStorage:LoggerName"] ?? "SampleApp";
var traceId = configuration["AzureTableStorage:TraceId"] ?? "sample-trace-id";

Console.WriteLine($"Connecting to Azure Table Storage: {tableName}");

// Setup Dependency Injection
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());
services.AddAzureTableStorageLogging(connectionString, tableName, loggerName);
var serviceProvider = services.BuildServiceProvider();

// Create cancellation token with timeout
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var cancellationToken = cts.Token;

try
{
    // Get the AzureTableStorageLogger for async operations
    var logger = serviceProvider.GetRequiredService<AzureTableStorageLogger>();

    // Log initialization message
    Console.WriteLine("Logging sample messages...");

    await logger.InformationAsync(
        "Application started",
        traceId: traceId,
        metadata: new Dictionary<string, object>
        {
            { "version", "1.0.0" },
            { "environment", "Development" }
        },
        cancellationToken: cancellationToken
    );

    await logger.WarningAsync(
        "This is a warning message",
        metadata: new Dictionary<string, object>
        {
            { "severity", "medium" }
        },
        cancellationToken: cancellationToken
    );

    await logger.InformationAsync(
        "Operation completed",
        traceId: "trace-123",
        cancellationToken: cancellationToken
    );

    // Retrieve logs
    Console.WriteLine("\nRetrieving logs...");

    var query = new LogQuery
    {
        PageSize = 10,
        OrderBy = "Timestamp",
        Ascending = false
    };

    var (logs, _) = await logger.GetLogsAsync(query, cancellationToken);

    Console.WriteLine($"\nFound {logs.Count} log entries:");

    foreach (var log in logs)
    {
        Console.WriteLine($"[{log.Level}] {log.Timestamp:u} - {log.Message}");
        if (log.Metadata != null)
        {
            Console.WriteLine($"  Metadata: {log.Metadata}");
        }
    }

    Console.WriteLine("\nSample completed successfully!");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation timed out.");
    Environment.Exit(1);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}
