namespace MZ.Logging.AzureTableStorage;

/// <summary>
/// Provides API documentation exports for the logging library.
/// </summary>
public class ApiDocumentation
{
    /// <summary>
    /// Gets a summary of the logging library's main components.
    /// </summary>
    /// <returns>API summary documentation.</returns>
    public static string GetApiSummary()
    {
        return @"
# MZ.Logging.AzureTableStorage API Reference

## Core Classes

### AzureTableStorageLogger
Professional logger for Azure Table Storage with structured logging and distributed tracing.

#### Methods
- `DebugAsync()` - Logs debug level messages
- `InformationAsync()` - Logs information level messages
- `WarningAsync()` - Logs warning level messages
- `ErrorAsync()` - Logs error level messages
- `CriticalAsync()` - Logs critical level messages
- `GetLogsAsync()` - Retrieves logs with filtering
- `GetLogEntryAsync()` - Retrieves a single log entry

### AzureTableStorage
Implementation of ILogStorage for Azure Table Storage operations.

#### Methods
- `StoreLogAsync()` - Stores a log entry
- `GetLogsAsync()` - Retrieves logs with pagination
- `GetLogEntryAsync()` - Retrieves a single log entry

## Interfaces

### ILogStorage
Contract for log storage implementations.

### ILogValidator
Contract for log validation implementations.

## Models

### LogEntry
Represents a complete log entry with metadata.

### LogLevel
Enumeration for log levels (Debug, Information, Warning, Error, Critical).

### LogQuery
Represents a query for retrieving logs with filtering and pagination.

## Extensions

### ServiceCollectionExtensions
Provides dependency injection extensions for registering logging services.

## Exceptions

### LoggingException
Base exception for logging operations.

### StorageException
Exception for storage operation failures.

### ValidationException
Exception for validation failures.
";
    }
}
