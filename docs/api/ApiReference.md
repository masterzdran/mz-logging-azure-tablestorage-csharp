# API Reference

Complete API documentation for MZ.Logging.AzureTableStorage

---

## Table of Contents

1. [Core Logging](#core-logging)
   - [AzureTableStorageLogger](#azuretablestoragelogger)
   - [ILogger Integration](#ilogger-integration)
2. [Storage](#storage)
   - [ILogStorage](#ilogstorage)
   - [AzureTableStorage](#azuretablestorage)
3. [Models](#models)
   - [LogEntry](#logentry)
   - [LogQuery](#logquery)
   - [ValidationResult](#validationresult)
4. [Configuration](#configuration)
   - [AzureTableStorageLoggingConfiguration](#azuretablestorageloggingconfiguration)
   - [Configuration Extensions](#configuration-extensions)
5. [Validation](#validation)
   - [ILogValidator](#ilogvalidator)
6. [Dependency Injection](#dependency-injection)
   - [ServiceCollectionExtensions](#servicecollectionextensions)
7. [Exceptions](#exceptions)

---

## Core Logging

### AzureTableStorageLogger

Main logger class with async operations for structured logging.

#### Constructor

```csharp
public AzureTableStorageLogger(
    ILogStorage storage,
    string loggerName,
    string defaultTraceId,
    ILogValidator? validator = null)
```

**Parameters:**

- `storage` - The log storage implementation
- `loggerName` - The name of the logger (used as partition key)
- `defaultTraceId` - Default trace ID for correlation
- `validator` - Optional custom validator (uses `LogValidator` by default)

**Exceptions:**

- `ArgumentNullException` - When storage is null
- `ArgumentException` - When loggerName or defaultTraceId is empty

#### Methods

##### DebugAsync

```csharp
public async Task DebugAsync(
    string message,
    string? traceId = null,
    Exception? exception = null,
    Dictionary<string, object>? metadata = null,
    CancellationToken cancellationToken = default)
```

Logs a debug-level message asynchronously.

**Parameters:**

- `message` - The log message (required, max 4000 chars after sanitization)
- `traceId` - Optional trace ID (uses default if not provided)
- `exception` - Optional exception to log
- `metadata` - Optional key-value metadata
- `cancellationToken` - Cancellation token

**Example:**

```csharp
await logger.DebugAsync(
    "Processing item",
    traceId: "trace-123",
    metadata: new Dictionary<string, object> { { "itemId", 42 } }
);
```

---

##### InformationAsync

```csharp
public async Task InformationAsync(
    string message,
    string? traceId = null,
    Exception? exception = null,
    Dictionary<string, object>? metadata = null,
    CancellationToken cancellationToken = default)
```

Logs an information-level message asynchronously.

**Example:**

```csharp
await logger.InformationAsync(
    "User logged in",
    metadata: new Dictionary<string, object>
    {
        { "userId", "user-123" },
        { "ipAddress", "192.168.1.1" }
    }
);
```

---

##### WarningAsync

```csharp
public async Task WarningAsync(
    string message,
    string? traceId = null,
    Exception? exception = null,
    Dictionary<string, object>? metadata = null,
    CancellationToken cancellationToken = default)
```

Logs a warning-level message asynchronously.

**Example:**

```csharp
await logger.WarningAsync(
    "Cache miss detected",
    metadata: new Dictionary<string, object> { { "cacheKey", "user:123" } }
);
```

---

##### ErrorAsync

```csharp
public async Task ErrorAsync(
    string message,
    string? traceId = null,
    Exception? exception = null,
    Dictionary<string, object>? metadata = null,
    CancellationToken cancellationToken = default)
```

Logs an error-level message asynchronously.

**Example:**

```csharp
try
{
    await ProcessOrderAsync(order);
}
catch (Exception ex)
{
    await logger.ErrorAsync(
        "Failed to process order",
        exception: ex,
        metadata: new Dictionary<string, object> { { "orderId", order.Id } }
    );
}
```

---

##### CriticalAsync

```csharp
public async Task CriticalAsync(
    string message,
    string? traceId = null,
    Exception? exception = null,
    Dictionary<string, object>? metadata = null,
    CancellationToken cancellationToken = default)
```

Logs a critical-level message asynchronously.

**Example:**

```csharp
await logger.CriticalAsync(
    "Database connection lost",
    exception: dbException,
    metadata: new Dictionary<string, object> { { "retryCount", 5 } }
);
```

---

##### GetLogsAsync

```csharp
public async Task<(IReadOnlyList<LogEntry>, string?)> GetLogsAsync(
    LogQuery query,
    CancellationToken cancellationToken = default)
```

Retrieves logs asynchronously with filtering and pagination.

**Parameters:**

- `query` - Query parameters (filters, sorting, pagination)
- `cancellationToken` - Cancellation token

**Returns:**

- Tuple of (log entries, continuation token for next page)

**Example:**

```csharp
var query = new LogQuery
{
    PageSize = 50,
    OrderBy = "Timestamp",
    Ascending = false,
    Filters = new Dictionary<string, object>
    {
        { "LogLevel", "Error" },
        { "TraceId", "trace-123" }
    }
};

var (logs, nextToken) = await logger.GetLogsAsync(query);

foreach (var log in logs)
{
    Console.WriteLine($"[{log.Level}] {log.Message}");
}

// Get next page if available
if (nextToken != null)
{
    query.ContinuationToken = nextToken;
    var (moreLogs, _) = await logger.GetLogsAsync(query);
}
```

---

### ILogger Integration

The library provides full `Microsoft.Extensions.Logging.ILogger` compatibility through the `AzureTableStorageLoggerProvider` and `AzureTableStorageLoggerWrapper`.

#### Using ILogger<T>

```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public async Task DoWorkAsync()
    {
        _logger.LogInformation("Starting work");
        _logger.LogDebug("Processing item {ItemId}", 123);
        _logger.LogError(exception, "Failed to process");
    }
}
```

#### Supported ILogger Methods

All standard `ILogger` extension methods are supported:

- `LogTrace()` / `LogDebug()` → Debug level
- `LogInformation()` → Information level
- `LogWarning()` → Warning level
- `LogError()` → Error level
- `LogCritical()` → Critical level

#### EventId Support

EventIds are automatically captured and stored in metadata:

```csharp
logger.LogInformation(
    new EventId(1001, "OrderCreated"),
    "Order {OrderId} created",
    orderId
);
// EventId and EventName stored in metadata
```

---

## Storage

### ILogStorage

Storage abstraction interface for log persistence.

```csharp
public interface ILogStorage
{
    Task StoreLogAsync(
        string partitionKey,
        string rowKey,
        LogEntry logEntry,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<LogEntry> Logs, string? ContinuationToken)> GetLogsAsync(
        LogQuery query,
        CancellationToken cancellationToken = default);

    Task<LogEntry?> GetLogEntryAsync(
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken = default);
}
```

**Purpose:**

- Abstracts storage implementation for testability
- Enables alternative storage backends
- Facilitates unit testing with mocks

---

### AzureTableStorage

Azure Table Storage implementation of `ILogStorage`.

#### Constructor

```csharp
public AzureTableStorage(
    string connectionString,
    string tableName,
    ILogValidator? validator = null)
```

**Parameters:**

- `connectionString` - Azure Storage connection string or "UseDevelopmentStorage=true"
- `tableName` - Table name for logs
- `validator` - Optional custom validator

**Features:**

- Automatic table creation
- Input sanitization (removes control characters, enforces length limits)
- OData injection prevention
- Pagination support
- Error handling and retry logic

#### Table Schema

| Property     | Type   | Description                                  |
| ------------ | ------ | -------------------------------------------- |
| PartitionKey | String | Logger name                                  |
| RowKey       | String | Timestamp + GUID (yyyyMMddHHmmssffffff_guid) |
| LogLevel     | String | Log level (Debug, Information, etc.)         |
| Message      | String | Sanitized log message (max 4000 chars)       |
| Timestamp    | String | ISO 8601 timestamp                           |
| TraceId      | String | Trace/correlation ID                         |
| LoggerName   | String | Logger name                                  |
| Location     | String | Caller location (file:line)                  |
| Exception    | String | Exception details                            |
| Metadata     | String | JSON serialized metadata                     |

---

## Models

### LogEntry

Represents a complete log entry with metadata.

```csharp
public class LogEntry
{
    public required string PartitionKey { get; set; }
    public required string RowKey { get; set; }
    public LogLevel Level { get; set; }
    public required string Message { get; set; }
    public required string Timestamp { get; set; }
    public string? TraceId { get; set; }
    public required string LoggerName { get; set; }
    public required string Location { get; set; }
    public string? Metadata { get; set; }
    public string? Exception { get; set; }

    public ValidationResult Validate();
}
```

**Properties:**

- `PartitionKey` - Partition key (typically logger name)
- `RowKey` - Unique row identifier
- `Level` - Log level enum
- `Message` - Log message (required, non-empty)
- `Timestamp` - ISO 8601 timestamp string
- `TraceId` - Optional trace/correlation ID
- `LoggerName` - Logger name (required)
- `Location` - Caller location info
- `Metadata` - JSON string of metadata
- `Exception` - Exception details string

**Validation Rules:**

- All required fields must be non-empty
- Timestamp must be valid ISO 8601 format
- Message cannot be whitespace only

---

### LogQuery

Query builder for retrieving logs with filtering and pagination.

```csharp
public class LogQuery
{
    public int PageSize { get; set; } = 50;
    public string? ContinuationToken { get; set; }
    public string OrderBy { get; set; } = "Timestamp";
    public bool Ascending { get; set; } = false;
    public Dictionary<string, object> Filters { get; set; } = new();

    public ValidationResult Validate();
}
```

**Properties:**

- `PageSize` - Number of results per page (1-1000, default: 50)
- `ContinuationToken` - Token for next page (from previous query)
- `OrderBy` - Field to sort by (Timestamp, LogLevel, TraceId, LoggerName, Location, Message)
- `Ascending` - Sort direction (false = descending)
- `Filters` - Key-value filter conditions

**Supported Filter Types:**

- `string` - Exact match (with OData injection prevention)
- `int`, `long` - Numeric equality
- `double` - Numeric equality with 'd' suffix
- `bool` - Boolean equality
- `DateTimeOffset` - Date range (same day)

**Example:**

```csharp
var query = new LogQuery
{
    PageSize = 100,
    OrderBy = "Timestamp",
    Ascending = false,
    Filters = new Dictionary<string, object>
    {
        { "LogLevel", "Error" },
        { "LoggerName", "MyService" },
        { "TraceId", "trace-123" }
    }
};
```

---

### ValidationResult

Result of a validation operation.

```csharp
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
```

**Properties:**

- `IsValid` - Whether validation passed
- `Errors` - List of validation error messages

---

## Configuration

### AzureTableStorageLoggingConfiguration

Configuration model for logging settings.

```csharp
public class AzureTableStorageLoggingConfiguration
{
    public string? ConnectionString { get; set; }
    public string? TableName { get; set; }
    public string? LoggerName { get; set; }
    public string? DefaultTraceId { get; set; }
    public string? EnvironmentName { get; set; }

    public List<string> Validate();
}
```

**Properties:**

- `ConnectionString` - Azure Storage connection string (required)
- `TableName` - Table name for logs (required)
- `LoggerName` - Default logger name (required)
- `DefaultTraceId` - Default trace ID (optional)
- `EnvironmentName` - Environment identifier (optional)

**Usage with appsettings.json:**

```json
{
  "AzureTableStorageLogging": {
    "ConnectionString": "DefaultEndpointsProtocol=https;...",
    "TableName": "logs",
    "LoggerName": "MyApp",
    "DefaultTraceId": "default",
    "EnvironmentName": "Production"
  }
}
```

---

### Configuration Extensions

#### ConfigurationBuilderExtensions

Extension methods for adding configuration sources.

##### AddAzureKeyVault

```csharp
public static IConfigurationBuilder AddAzureKeyVault(
    this IConfigurationBuilder builder,
    string keyVaultUri)
```

Adds Azure Key Vault as a configuration source.

**Example:**

```csharp
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddAzureKeyVault("https://myvault.vault.azure.net")
    .Build();
```

##### AddAzureAppConfiguration

```csharp
public static IConfigurationBuilder AddAzureAppConfiguration(
    this IConfigurationBuilder builder,
    string connectionString)
```

Adds Azure App Configuration as a configuration source.

**Example:**

```csharp
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddAzureAppConfiguration("Endpoint=https://...;Id=...;Secret=...")
    .Build();
```

##### GetAzureTableStorageLoggingConfiguration

```csharp
public static AzureTableStorageLoggingConfiguration GetAzureTableStorageLoggingConfiguration(
    this IConfiguration configuration,
    string sectionName = "AzureTableStorageLogging")
```

Loads and validates logging configuration from IConfiguration.

**Example:**

```csharp
var loggingConfig = configuration.GetAzureTableStorageLoggingConfiguration();

services.AddAzureTableStorageLogging(
    loggingConfig.ConnectionString,
    loggingConfig.TableName,
    loggingConfig.LoggerName
);
```

---

## Validation

### ILogValidator

Contract for log validation implementations.

```csharp
public interface ILogValidator
{
    ValidationResult Validate(LogEntry logEntry);
}
```

**Default Implementation:** `LogValidator`

**Purpose:**

- Validate log entries before storage
- Enable custom validation logic
- Ensure data integrity

**Custom Validator Example:**

```csharp
public class CustomLogValidator : ILogValidator
{
    public ValidationResult Validate(LogEntry logEntry)
    {
        var result = logEntry.Validate(); // Call base validation

        // Add custom validation
        if (logEntry.Message.Contains("sensitive"))
        {
            result.IsValid = false;
            result.Errors.Add("Log contains sensitive data");
        }

        return result;
    }
}

// Use custom validator
var logger = new AzureTableStorageLogger(
    storage,
    "MyApp",
    "trace-id",
    new CustomLogValidator()
);
```

---

## Dependency Injection

### ServiceCollectionExtensions

Extension methods for registering logging services.

#### AddAzureTableStorageLogging

```csharp
public static IServiceCollection AddAzureTableStorageLogging(
    this IServiceCollection services,
    string connectionString,
    string tableName,
    string loggerName,
    string defaultTraceId = "default-trace-id")
```

Registers all logging services with the DI container.

**Registered Services:**

- `ILogStorage` → `AzureTableStorage` (singleton)
- `AzureTableStorageLoggerProvider` (singleton)
- `AzureTableStorageLogger` (singleton)
- `ILoggerProvider` → `AzureTableStorageLoggerProvider`
- `ILogger<T>` → via logging framework

**Parameters:**

- `connectionString` - Azure Storage connection string
- `tableName` - Table name for logs
- `loggerName` - Logger name (used as partition key)
- `defaultTraceId` - Default trace ID (optional, default: "default-trace-id")

**Example:**

```csharp
services.AddAzureTableStorageLogging(
    "UseDevelopmentStorage=true",
    "logs",
    "MyApp",
    "default-trace"
);

// Services can now inject:
// - ILogger<T>
// - ILogger
// - AzureTableStorageLogger
```

---

## Exceptions

### LoggingException

Base exception for logging operations.

```csharp
public class LoggingException : Exception
{
    public LoggingException(string message);
    public LoggingException(string message, Exception innerException);
}
```

### StorageException

Exception for storage operation failures.

```csharp
public class StorageException : LoggingException
{
    public StorageException(string message);
    public StorageException(string message, Exception innerException);
}
```

**Thrown when:**

- Azure Table Storage operations fail
- Network errors occur
- Storage account is inaccessible

### ValidationException

Exception for validation failures.

```csharp
public class ValidationException : LoggingException
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(string message, IReadOnlyList<string> errors);
}
```

**Thrown when:**

- Log entry validation fails
- Query validation fails
- Configuration validation fails

**Example Handling:**

```csharp
try
{
    await logger.InformationAsync("");  // Empty message
}
catch (ValidationException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"  - {error}");
    }
}
catch (StorageException ex)
{
    Console.WriteLine($"Storage error: {ex.Message}");
}
```

---

## Security Considerations

### Input Sanitization

All inputs are automatically sanitized:

- Control characters removed
- Message length limited to 4000 characters
- Special characters escaped in filters

### OData Injection Prevention

Filter values are escaped to prevent OData injection attacks:

```csharp
// Single quotes are automatically escaped
query.Filters["Message"] = "user's input";  // Safe: "user''s input"
```

### Secure Exception Handling

Exceptions don't leak sensitive information:

- No connection strings in error messages
- No PII in logs (unless explicitly added)
- Safe error messages for production

---

## Performance Tips

1. **Use Async Methods** - Always await async operations
2. **Batch Queries** - Use appropriate PageSize for your needs
3. **Use Direct Logger** - For high-throughput, use `AzureTableStorageLogger` directly
4. **Connection Pooling** - Automatic via Azure SDK
5. **Pagination** - Use continuation tokens for large result sets

---

## Migration from Python Version

Key differences:

| Feature | Python Version | C# Version                               |
| ------- | -------------- | ---------------------------------------- |
| Async   | `asyncio`      | `async/await`                            |
| Typing  | Type hints     | Nullable reference types                 |
| DI      | Manual         | Microsoft.Extensions.DependencyInjection |
| Logging | Custom         | Microsoft.Extensions.Logging             |
| Config  | Dict/ENV       | IConfiguration, Key Vault, App Config    |
| Testing | pytest         | xUnit, Moq, FluentAssertions             |

---

For more examples and guides, see the [documentation](../README.md).
