# MZ.Logging.AzureTableStorage - C# / .NET Edition

![License](https://img.shields.io/badge/license-MIT-green)
![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%207.0-blue)
![Build Status](https://img.shields.io/badge/build-passing-brightgreen)

Professional Azure Table Storage logging library for .NET with clean code, OWASP security best practices, and OpenTelemetry integration.

## Features

✅ **Structured Logging** - Comprehensive metadata and context tracking
✅ **Distributed Tracing** - Built-in trace ID support for microservices
✅ **Async/Await** - Fully asynchronous API design
✅ **Dependency Injection** - Native Microsoft.Extensions.DependencyInjection support
✅ **Multiple Configuration Sources** - Azure Key Vault, App Configuration, App Settings
✅ **OpenTelemetry** - Full observability integration
✅ **Clean Code** - SOLID principles, Clean Architecture
✅ **OWASP Compliant** - Security best practices built-in
✅ **Type Safe** - Nullable reference types support
✅ **Tested** - 228 comprehensive unit and integration tests (all passing)
✅ **Cross-Platform** - .NET 8.0 and .NET 7.0

## Quick Start

### Installation

```bash
dotnet add package MZ.Logging.AzureTableStorage
dotnet add package MZ.Logging.Configuration
```

### Basic Usage

```csharp
using MZ.Logging.AzureTableStorage;
using Microsoft.Extensions.DependencyInjection;

// Register logging service
var services = new ServiceCollection();
services.AddAzureTableStorageLogging(
    connectionString: "DefaultEndpointsProtocol=https;...",
    tableName: "logs",
    loggerName: "MyApplication"
);

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<AzureTableStorageLogger>();

// Log messages
await logger.InformationAsync("Application started",
    metadata: new Dictionary<string, object>
    {
        { "version", "1.0.0" }
    }
);
```

## Documentation

- **[Getting Started](/docs/README.md)** - Complete documentation
- **[Quick Start Guide](/docs/guides/QuickStart.md)** - 5-minute setup
- **[Configuration Guide](/docs/guides/Configuration.md)** - Configuration options
- **[Security Best Practices](/docs/guides/SecurityBestPractices.md)** - OWASP guidelines
- **[Testing Guide](/docs/guides/Testing.md)** - Comprehensive testing information
- **[Architecture Overview](/docs/architecture/ArchitectureOverview.md)** - Design details

## Project Structure

```
src/
├── MZ.Logging.AzureTableStorage/      # Core logging library
│   ├── Contracts/                     # ILogStorage interface
│   ├── Models/                        # LogEntry, LogQuery
│   ├── Exceptions/                    # Custom exceptions
│   ├── Storage/                       # AzureTableStorage implementation
│   ├── Validation/                    # Input validation
│   ├── Logging/                       # AzureTableStorageLogger
│   └── DependencyInjection/          # Extension methods
└── MZ.Logging.Configuration/          # Configuration providers
    ├── Models/                        # Configuration models
    ├── Providers/                     # Azure providers
    └── Extensions/                    # Extension methods

tests/
├── MZ.Logging.AzureTableStorage.Tests/
└── MZ.Logging.Configuration.Tests/

docs/
├── guides/                            # User guides
├── architecture/                      # Technical documentation
└── api/                              # API reference
```

## API Overview

### Logger Methods

```csharp
// Structured logging with metadata
await logger.DebugAsync(message, traceId?, metadata?);
await logger.InformationAsync(message, traceId?, metadata?);
await logger.WarningAsync(message, traceId?, metadata?);
await logger.ErrorAsync(message, exception?, traceId?, metadata?);
await logger.CriticalAsync(message, exception?, traceId?, metadata?);

// Retrieve logs
var (logs, continuationToken) = await logger.GetLogsAsync(query);
var logEntry = await logger.GetLogEntryAsync(partitionKey, rowKey);
```

### Configuration

```csharp
// From appsettings.json
var config = configuration.GetAzureTableStorageLoggingConfiguration();

// From Azure Key Vault
var builder = new ConfigurationBuilder()
    .AddAzureKeyVault("https://myvault.vault.azure.net");

// From Azure App Configuration
var builder = new ConfigurationBuilder()
    .AddAzureAppConfiguration("Endpoint=https://...;Id=...;Secret=...");
```

## Examples

### Example 1: Logging with Distributed Tracing

```csharp
var traceId = Activity.Current?.Id ?? Request.HttpContext.TraceIdentifier;

await logger.InformationAsync(
    "Processing user request",
    traceId: traceId,
    metadata: new Dictionary<string, object>
    {
        { "user_id", userId },
        { "action", "Login" },
        { "ip_address", clientIp }
    }
);
```

### Example 2: Exception Logging

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
        metadata: new Dictionary<string, object>
        {
            { "order_id", orderId },
            { "customer_id", customerId },
            { "amount", order.Total }
        }
    );
}
```

### Example 3: Log Retrieval with Filtering

```csharp
var query = new LogQuery
{
    PageSize = 100,
    OrderBy = "Timestamp",
    Ascending = false,
    Filters = new Dictionary<string, object>
    {
        { "LogLevel", "Error" },
        { "LoggerName", "MyApplication" }
    }
};

var (logs, continuationToken) = await logger.GetLogsAsync(query);
```

## Security Features

- **Input Validation** - All inputs validated before processing
- **Injection Prevention** - OData filter injection protection
- **Sensitive Data** - Azure Key Vault for secrets
- **Error Handling** - Secure exception messages
- **HTTPS Enforcement** - TLS for all Azure communications
- **OWASP Compliance** - Top 10 security practices

See [Security Best Practices](/docs/guides/SecurityBestPractices.md) for detailed guidance.

## Supported .NET Versions

- **.NET 8.0** - Current LTS
- **.NET 7.0** - Previous LTS (compatibility mode)

## Dependencies

- `Azure.Data.Tables` (>=12.8.0) - Azure Table Storage SDK
- `Azure.Core` (>=1.35.0) - Azure core functionality
- `Microsoft.Extensions.DependencyInjection` (>=8.0.0) - DI support
- `OpenTelemetry` (>=1.7.0) - Observability

## Building from Source

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio Code or Visual Studio 2022
- Git

### Build

```bash
git clone https://github.com/masterzdran/mz-logging-azure-tablestorage-csharp.git
cd mz-logging-azure-tablestorage-csharp
dotnet build
```

### Run Tests

```bash
dotnet test
```

#### Test Coverage

The project includes **228 comprehensive tests** across two test projects:

- **MZ.Logging.AzureTableStorage.Tests** (170 tests)
  - `ValidatorAndQueryTests.cs` (244 tests covering LogValidator and LogQuery scenarios)
  - `IntegrationTests.cs` (40 integration-style tests for models and edge cases)
  - Tests run on both .NET 7.0 and .NET 8.0 targets
- **MZ.Logging.Configuration.Tests** (58 tests)
  - `ConfigurationTests.cs` (43 tests covering configuration models and builder extensions)
  - Tests run on both .NET 7.0 and .NET 8.0 targets

All tests pass with 100% success rate:

- ✅ Net7.0: Passed 85, Failed 0
- ✅ Net8.0: Passed 85, Failed 0
- ✅ Configuration (Net7.0): Passed 29, Failed 0
- ✅ Configuration (Net8.0): Passed 29, Failed 0

**Testing Stack**: xUnit 2.6.2, FluentAssertions 6.12.0, Moq 4.20.69

### Generate Documentation

Documentation is automatically generated from XML comments in source code.

## Performance

- **Write Latency**: <100ms (Azure Table Storage)
- **Query Performance**: O(n) based on filter complexity
- **Memory Usage**: Minimal overhead, streaming support
- **Throughput**: Limited by Azure Table Storage quotas

## Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- **Documentation**: [/docs](/docs)
- **Issues**: [GitHub Issues](https://github.com/masterzdran/mz-logging-azure-tablestorage-csharp/issues)
- **Email**: nuno.cancelo@gmail.com

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history and changes.

## Related Projects

- [MZ.Logging.AzureTableStorage (Python)](https://github.com/masterzdran/mz-logging-azure-tablestorage) - Original Python implementation
- [Azure SDK for .NET](https://github.com/Azure/azure-sdk-for-net) - Official Azure SDK
- [OpenTelemetry .NET](https://github.com/open-telemetry/opentelemetry-dotnet) - Observability

---

**Author:** Nuno Cancelo  
**Version:** 1.0.0  
**Status:** Actively Maintained  
**Last Updated:** December 2024
