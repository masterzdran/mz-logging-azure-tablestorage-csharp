# MZ.Logging.AzureTableStorage - C# / .NET Documentation

## Table of Contents

1. [Overview](#overview)
2. [Getting Started](#getting-started)
3. [Architecture](#architecture)
4. [API Reference](#api-reference)
5. [Configuration](#configuration)
6. [Security & OWASP Best Practices](#security--owasp-best-practices)
7. [OpenTelemetry Integration](#opentelemetry-integration)
8. [Examples](#examples)
9. [Testing](#testing)
10. [Migration from Python](#migration-from-python)

## Overview

**MZ.Logging.AzureTableStorage** is a professional-grade, enterprise-ready logging library for .NET that provides structured logging to Azure Table Storage with full support for:

- **Distributed Tracing**: Built-in trace ID support for request correlation across microservices
- **Clean Code**: Follows SOLID principles, Clean Code architecture, and best practices
- **Security**: OWASP Top 10 secure implementation practices
- **Observability**: Full OpenTelemetry integration for metrics, traces, and structured logging
- **Dependency Injection**: Native support for Microsoft.Extensions.DependencyInjection
- **Configuration**: Multiple configuration sources (Azure App Configuration, Key Vault, App Settings)
- **Async/Await**: Fully asynchronous API design
- **Type Safety**: Complete nullable reference types support

### Key Differences from Python Version

- Compiled language with compile-time type checking
- Synchronous and asynchronous APIs
- Better performance through native compilation
- Enhanced security through CLR runtime protections
- Better enterprise integration capabilities

## Getting Started

### Installation

```bash
dotnet add package MZ.Logging.AzureTableStorage
dotnet add package MZ.Logging.Configuration
```

### Basic Usage

```csharp
using MZ.Logging.AzureTableStorage;
using Microsoft.Extensions.DependencyInjection;

// Create service collection
var services = new ServiceCollection();

// Register logging service
services.AddAzureTableStorageLogging(
    connectionString: "DefaultEndpointsProtocol=https;...",
    tableName: "logs",
    loggerName: "MyService",
    defaultTraceId: "optional-default-trace-id"
);

var serviceProvider = services.BuildServiceProvider();

// Get logger instance
var logger = serviceProvider.GetRequiredService<AzureTableStorageLogger>();

// Log messages
await logger.InformationAsync("Application started", metadata: new Dictionary<string, object>
{
    { "environment", "production" },
    { "version", "1.0.0" }
});
```

## Architecture

### Project Structure

```
src/
├── MZ.Logging.AzureTableStorage/      # Core logging library
│   ├── Contracts/                     # Interfaces and contracts
│   ├── Models/                        # Data models
│   ├── Exceptions/                    # Custom exceptions
│   ├── Storage/                       # Storage implementations
│   ├── Validation/                    # Input validation
│   ├── Logging/                       # Logger implementations
│   └── DependencyInjection/          # DI extensions
└── MZ.Logging.Configuration/          # Configuration providers
    ├── Models/                        # Configuration models
    ├── Providers/                     # Azure configuration providers
    └── Extensions/                    # Configuration extensions

tests/
├── MZ.Logging.AzureTableStorage.Tests/
└── MZ.Logging.Configuration.Tests/

docs/
├── README.md
├── guides/
│   ├── QuickStart.md
│   ├── Configuration.md
│   └── SecurityBestPractices.md
├── architecture/
│   └── ArchitectureOverview.md
└── api/
    └── ApiReference.md
```

### Core Components

#### 1. **ILogStorage**

Abstraction for log storage implementations. Allows for flexible storage backends.

```csharp
public interface ILogStorage
{
    Task StoreLogAsync(string partitionKey, string rowKey, LogEntry logEntry, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<LogEntry> Logs, string? ContinuationToken)> GetLogsAsync(LogQuery query, CancellationToken cancellationToken = default);
    Task<LogEntry?> GetLogEntryAsync(string partitionKey, string rowKey, CancellationToken cancellationToken = default);
}
```

#### 2. **AzureTableStorage**

Production-grade Azure Table Storage implementation with validation and error handling.

Features:

- Automatic table creation
- Input sanitization
- Filter building with injection prevention
- Pagination support
- Comprehensive error handling

#### 3. **AzureTableStorageLogger**

Main logger class with structured logging capabilities.

Features:

- Async/await API
- Automatic caller location tracking
- Distributed tracing support
- Metadata enrichment
- Exception handling
- OpenTelemetry integration

#### 4. **Configuration Providers**

Support for multiple Azure configuration sources:

- **Azure Key Vault**: For sensitive secrets
- **Azure App Configuration**: For feature flags and settings
- **Azure App Settings**: For environment-specific configuration

## API Reference

### AzureTableStorageLogger

#### Debug Logging

```csharp
public async Task DebugAsync(
    string message,
    string? traceId = null,
    Dictionary<string, object>? metadata = null,
    CancellationToken cancellationToken = default)
```

#### Information Logging

```csharp
public async Task InformationAsync(
    string message,
    string? traceId = null,
    Dictionary<string, object>? metadata = null,
    CancellationToken cancellationToken = default)
```

#### Warning Logging

```csharp
public async Task WarningAsync(
    string message,
    string? traceId = null,
    Dictionary<string, object>? metadata = null,
    CancellationToken cancellationToken = default)
```

#### Error Logging

```csharp
public async Task ErrorAsync(
    string message,
    Exception? exception = null,
    string? traceId = null,
    Dictionary<string, object>? metadata = null,
    CancellationToken cancellationToken = default)
```

#### Critical Logging

```csharp
public async Task CriticalAsync(
    string message,
    Exception? exception = null,
    string? traceId = null,
    Dictionary<string, object>? metadata = null,
    CancellationToken cancellationToken = default)
```

#### Retrieve Logs

```csharp
public async Task<(IReadOnlyList<LogEntry> Logs, string? ContinuationToken)> GetLogsAsync(
    LogQuery query,
    CancellationToken cancellationToken = default)

public async Task<LogEntry?> GetLogEntryAsync(
    string partitionKey,
    string rowKey,
    CancellationToken cancellationToken = default)
```

### LogQuery

```csharp
public class LogQuery
{
    public int PageSize { get; set; } = 50;
    public string? ContinuationToken { get; set; }
    public string OrderBy { get; set; } = "Timestamp";
    public bool Ascending { get; set; }
    public Dictionary<string, object> Filters { get; set; } = new();

    public ValidationResult Validate() { /* ... */ }
}
```

### LogEntry

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

    public ValidationResult Validate() { /* ... */ }
}
```

## Configuration

### Dependency Injection Setup

```csharp
services.AddAzureTableStorageLogging(
    connectionString: "your-connection-string",
    tableName: "logs",
    loggerName: "MyApplication",
    defaultTraceId: "optional-trace-id"
);
```

### Configuration from App Settings (appsettings.json)

```json
{
  "AzureTableStorageLogging": {
    "ConnectionString": "DefaultEndpointsProtocol=https;...",
    "TableName": "logs",
    "LoggerName": "MyApplication",
    "DefaultTraceId": "app-trace",
    "EnvironmentName": "Production"
  }
}
```

### Azure App Configuration

```csharp
var builder = new ConfigurationBuilder()
    .AddAzureAppConfiguration(connectionString);

var configuration = builder.Build();
var loggingConfig = configuration.GetAzureTableStorageLoggingConfiguration();
```

### Azure Key Vault

```csharp
var builder = new ConfigurationBuilder()
    .AddAzureKeyVault("https://myvault.vault.azure.net");

var configuration = builder.Build();
```

## Security & OWASP Best Practices

### 1. Input Validation (CWE-20)

- All inputs validated before processing
- LogEntry model includes built-in validation
- LogQuery validates page size (1-1000)
- Field names validated against whitelist

### 2. Injection Prevention (CWE-89)

- OData filter injection protection
- Input escaping for filter values
- Table names validated
- No dynamic query construction

### 3. Sensitive Data Protection (CWE-200)

- Connection string validation before storage
- Secure exception messages (no sensitive data in exceptions)
- Support for Azure Key Vault for secret management
- Metadata sanitization

### 4. Error Handling (CWE-209)

- Custom exception types for proper error context
- No stack traces exposed in public APIs
- Structured error logging
- Graceful degradation on failures

### 5. Resource Management

- Proper disposal of Azure clients
- Cancellation token support for all async operations
- Connection pooling
- Timeouts on network operations

### 6. Cryptographic Security

- TLS enforcement for Azure connections
- Default Azure SDK security practices
- Support for Managed Identity authentication

### 7. Access Control (CWE-276)

- Dependency Injection for testability
- Interface-based design for flexibility
- Role-Based Access Control via Azure roles
- Audit logging for all operations

## OpenTelemetry Integration

### Activity Tracing

The logger automatically creates OpenTelemetry activities for tracing:

```csharp
using var activity = logger._activitySource.StartActivity("Operation");
activity?.SetTag("user.id", userId);
activity?.SetTag("order.id", orderId);
```

### Custom Instrumentation

```csharp
var metadata = new Dictionary<string, object>
{
    { "correlation_id", correlationId },
    { "user_id", userId },
    { "request_duration_ms", duration }
};

await logger.InformationAsync(
    "User login successful",
    traceId: correlationId,
    metadata: metadata
);
```

### Export to Observability Platforms

Configure OpenTelemetry exporters in your application:

```csharp
// Example with Azure Monitor
services.AddOpenTelemetry()
    .WithTracing(builder => builder
        .AddAzureMonitorTraceExporter(options => options.ConnectionString = "..."))
    .WithMetrics(builder => builder
        .AddAzureMonitorMetricExporter(options => options.ConnectionString = "..."));
```

## Examples

### Example 1: Basic Logging

```csharp
using MZ.Logging.AzureTableStorage;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddAzureTableStorageLogging(
    "DefaultEndpointsProtocol=https;AccountName=...",
    "logs",
    "MyApp"
);

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<AzureTableStorageLogger>();

// Log an info message
await logger.InformationAsync("Application started successfully");

// Log with metadata
await logger.InformationAsync(
    "User registered",
    metadata: new Dictionary<string, object>
    {
        { "user_id", "usr_123" },
        { "email", "user@example.com" }
    }
);
```

### Example 2: Error Logging with Exception

```csharp
try
{
    // Some operation
    await ProcessDataAsync();
}
catch (Exception ex)
{
    await logger.ErrorAsync(
        "Failed to process data",
        exception: ex,
        metadata: new Dictionary<string, object>
        {
            { "operation", "ProcessData" },
            { "retry_count", retryCount }
        }
    );
}
```

### Example 3: Distributed Tracing

```csharp
string traceId = Activity.Current?.Id ?? Request.HttpContext.TraceIdentifier;

await logger.InformationAsync(
    "Request received",
    traceId: traceId,
    metadata: new Dictionary<string, object>
    {
        { "method", "POST" },
        { "path", "/api/users" },
        { "client_ip", clientIp }
    }
);
```

### Example 4: Log Retrieval

```csharp
var query = new LogQuery
{
    PageSize = 100,
    OrderBy = "Timestamp",
    Ascending = false,
    Filters = new Dictionary<string, object>
    {
        { "LoggerName", "MyApp" },
        { "LogLevel", "Error" }
    }
};

var (logs, continuationToken) = await logger.GetLogsAsync(query);

foreach (var log in logs)
{
    Console.WriteLine($"{log.Timestamp} [{log.Level}] {log.Message}");
}
```

### Example 5: Configuration from Azure

```csharp
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddAzureAppConfiguration("Endpoint=https://...;Id=...;Secret=...")
    .AddAzureKeyVault("https://myvault.vault.azure.net")
    .Build();

var loggingConfig = config.GetAzureTableStorageLoggingConfiguration();

var services = new ServiceCollection();
services.AddAzureTableStorageLogging(
    loggingConfig.ConnectionString!,
    loggingConfig.TableName!,
    loggingConfig.LoggerName!,
    loggingConfig.DefaultTraceId
);
```

## Testing

### Test Framework & Infrastructure

The project uses a comprehensive testing stack for quality assurance:

- **Test Framework**: xUnit 2.6.2
- **Assertions**: FluentAssertions 6.12.0
- **Mocking**: Moq 4.20.69
- **Target Frameworks**: .NET 7.0 and .NET 8.0

### Test Structure

#### MZ.Logging.AzureTableStorage.Tests

**Total: 170 tests across both target frameworks**

- **ValidatorAndQueryTests.cs** (244 tests)
  - LogValidator comprehensive validation scenarios
  - LogQuery pagination and filtering tests
  - Edge cases and boundary conditions
  - Exception handling and error scenarios
- **IntegrationTests.cs** (40 tests)
  - Model integration and cross-component interactions
  - Complex validation scenarios
  - Data transformation and serialization
  - OpenTelemetry integration scenarios

#### MZ.Logging.Configuration.Tests

**Total: 58 tests across both target frameworks**

- **ConfigurationTests.cs** (43 tests)
  - AzureTableStorageLoggingConfiguration validation
  - Configuration builder extensions
  - Provider integration scenarios
  - Configuration loading from multiple sources

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run specific test project
dotnet test tests/MZ.Logging.AzureTableStorage.Tests/

# Run tests for specific framework
dotnet test -f net8.0
dotnet test -f net7.0
```

### Test Results

All tests pass with 100% success rate on both target frameworks:

```
MZ.Logging.AzureTableStorage.Tests (net7.0):  Passed 85, Failed 0
MZ.Logging.AzureTableStorage.Tests (net8.0):  Passed 85, Failed 0
MZ.Logging.Configuration.Tests (net7.0):      Passed 29, Failed 0
MZ.Logging.Configuration.Tests (net8.0):      Passed 29, Failed 0
```

### Test Naming Convention

Tests follow the standard naming pattern: `MethodName_Scenario_ExpectedResult`

Example: `LogValidator_ValidateWithValidEntry_ReturnsSuccess`

### Testing Best Practices Implemented

- ✅ All public APIs have comprehensive test coverage
- ✅ Unit tests for validation logic and edge cases
- ✅ Integration tests for cross-component interactions
- ✅ Exception handling and error scenario coverage
- ✅ Mocking external dependencies (Azure SDK)
- ✅ Fluent assertions for readable test failures
- ✅ Arrange-Act-Assert pattern consistency

## Migration from Python

### Comparison Table

| Feature       | Python                                    | C#                                               |
| ------------- | ----------------------------------------- | ------------------------------------------------ |
| Log Levels    | 5 (DEBUG, INFO, WARNING, ERROR, CRITICAL) | 5 (Debug, Information, Warning, Error, Critical) |
| Async         | asyncio                                   | async/await                                      |
| DI            | Manual                                    | Microsoft.Extensions.DependencyInjection         |
| Configuration | Manual JSON                               | Multiple providers                               |
| Storage       | Azure Tables SDK                          | Azure Tables SDK                                 |
| Validation    | Custom                                    | Built-in                                         |
| Observability | None                                      | OpenTelemetry                                    |
| Testing       | pytest                                    | xUnit + Moq                                      |

### API Mapping

| Python                        | C#                                   |
| ----------------------------- | ------------------------------------ |
| `await logger.info(...)`      | `await logger.InformationAsync(...)` |
| `await logger.error(...)`     | `await logger.ErrorAsync(...)`       |
| `await storage.get_logs(...)` | `await storage.GetLogsAsync(...)`    |
| `LogEntry()`                  | `new LogEntry { ... }`               |
| `ValidationException`         | `ValidationException`                |

---

**Version**: 1.0.0  
**License**: MIT  
**Repository**: https://github.com/masterzdran/mz-logging-azure-tablestorage-csharp
