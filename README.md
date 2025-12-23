# MZ.Logging.AzureTableStorage

> **Production-ready Azure Table Storage logging for .NET with Microsoft.Extensions.Logging support**

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%207.0-blue)](https://dotnet.microsoft.com/)
[![Tests](https://img.shields.io/badge/tests-114%20passing-brightgreen)](tests/)

A professional, enterprise-grade logging library that writes structured logs to Azure Table Storage with full support for distributed tracing, async operations, and Microsoft's logging abstractions.

---

## 🚀 Quick Start

### Installation

```bash
dotnet add package MZ.Logging.AzureTableStorage
```

### Basic Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MZ.Logging.AzureTableStorage;

// Setup with Dependency Injection
var services = new ServiceCollection();
services.AddAzureTableStorageLogging(
    connectionString: "DefaultEndpointsProtocol=https;AccountName=...",
    tableName: "logs",
    loggerName: "MyApp"
);

var serviceProvider = services.BuildServiceProvider();

// Use standard ILogger<T> - works like any other logging provider!
var logger = serviceProvider.GetRequiredService<ILogger<MyService>>();
logger.LogInformation("Application started");
```

---

## ✨ Key Features

- ✅ **Microsoft.Extensions.Logging** - Full `ILogger` and `ILogger<T>` support
- ✅ **Async/Await** - Non-blocking async operations throughout
- ✅ **Distributed Tracing** - Built-in trace ID support for microservices
- ✅ **Structured Logging** - Rich metadata and context tracking
- ✅ **Azure Integration** - Key Vault, App Configuration, Managed Identity
- ✅ **Type Safe** - Nullable reference types, compile-time safety
- ✅ **Security First** - OWASP best practices, input validation, injection prevention
- ✅ **Well Tested** - 114 unit & integration tests, 100% pass rate
- ✅ **Cross-Platform** - .NET 8.0 and .NET 7.0

---

## 📖 Usage Examples

### ASP.NET Core Application

```csharp
// In Program.cs
builder.Services.AddAzureTableStorageLogging(
    connectionString: builder.Configuration["AzureTableStorage:ConnectionString"],
    tableName: "logs",
    loggerName: "MyWebApi"
);

// In your controllers/services - standard DI injection
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> _logger;

    public OrderController(ILogger<OrderController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(Order order)
    {
        _logger.LogInformation("Creating order {OrderId}", order.Id);
        // Logs are automatically sent to Azure Table Storage
        return Ok();
    }
}
```

### Console Application

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MZ.Logging.AzureTableStorage;

var services = new ServiceCollection();
services.AddAzureTableStorageLogging(
    connectionString: "UseDevelopmentStorage=true", // Azurite for local dev
    tableName: "logs",
    loggerName: "ConsoleApp"
);

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Application started");
logger.LogWarning("Low memory detected");
logger.LogError(exception, "Failed to process {ItemId}", itemId);
```

### Advanced: Direct Async API

```csharp
// For high-performance scenarios, use the direct async logger
var logger = serviceProvider.GetRequiredService<AzureTableStorageLogger>();

await logger.InformationAsync(
    "User action performed",
    traceId: Activity.Current?.Id,
    metadata: new Dictionary<string, object>
    {
        { "userId", "12345" },
        { "action", "purchase" },
        { "amount", 99.99 }
    }
);

// Query logs
var query = new LogQuery
{
    PageSize = 50,
    OrderBy = "Timestamp",
    Filters = new Dictionary<string, object> { { "LogLevel", "Error" } }
};

var (logs, continuationToken) = await logger.GetLogsAsync(query);
```

### With Azure Key Vault

```csharp
using MZ.Logging.Configuration;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddAzureKeyVault("https://myvault.vault.azure.net")
    .Build();

var loggingConfig = config.GetAzureTableStorageLoggingConfiguration();

services.AddAzureTableStorageLogging(
    loggingConfig.ConnectionString,
    loggingConfig.TableName,
    loggingConfig.LoggerName
);
```

---

## 🏗️ Architecture

```
Your Application
    ↓
ILogger<T> / ILogger (Microsoft.Extensions.Logging)
    ↓
AzureTableStorageLoggerProvider (implements ILoggerProvider)
    ↓
AzureTableStorageLoggerWrapper (implements ILogger)
    ↓
AzureTableStorageLogger (async logging core)
    ↓
ILogStorage Interface
    ↓
AzureTableStorage Implementation
    ↓
Azure Table Storage
```

**Key Components:**

- **ILogger Integration**: Seamless Microsoft.Extensions.Logging compatibility
- **Async Core**: Non-blocking operations with proper async/await
- **Storage Abstraction**: Testable via `ILogStorage` interface
- **Security Layer**: Input validation, sanitization, injection prevention
- **Configuration**: Multiple sources (Key Vault, App Configuration, JSON)

---

## 📚 Documentation

### Quick Links

- **[📖 Documentation Index](DOCUMENTATION.md)** - Complete documentation guide and navigation
- **[🚀 Quick Start Guide](docs/guides/QuickStart.md)** - Get running in 5 minutes
- **[📘 Usage Guide](docs/guides/UsageGuide.md)** - Practical usage scenarios and examples

### Detailed Documentation

- **[🔧 Configuration Guide](docs/guides/Configuration.md)** - All configuration options and sources
- **[🔒 Security Best Practices](docs/guides/SecurityBestPractices.md)** - OWASP compliance guide
- **[🧪 Testing Guide](docs/guides/Testing.md)** - Testing strategies and best practices
- **[🏗️ Architecture Overview](docs/architecture/ArchitectureOverview.md)** - Deep dive into design
- **[📚 API Reference](docs/api/ApiReference.md)** - Complete API documentation
- **[📄 Complete Docs Hub](docs/README.md)** - Full documentation portal

---

## 🔧 Configuration

### appsettings.json

```json
{
  "AzureTableStorageLogging": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=...",
    "TableName": "logs",
    "LoggerName": "MyApplication",
    "DefaultTraceId": "default-trace",
    "EnvironmentName": "Production"
  }
}
```

### Azure Key Vault

Store sensitive configuration in Azure Key Vault:

- `AzureTableStorageLogging--ConnectionString`
- `AzureTableStorageLogging--TableName`

### Environment Variables

```bash
export AzureTableStorageLogging__ConnectionString="..."
export AzureTableStorageLogging__TableName="logs"
```

---

## 🔒 Security Features

- ✅ **Input Validation** - All inputs validated before processing
- ✅ **OData Injection Prevention** - Automatic escaping of filter values
- ✅ **Sanitization** - Control character removal, length limits
- ✅ **Secure Exceptions** - No sensitive data in error messages
- ✅ **Azure Key Vault** - Secrets management integration
- ✅ **Managed Identity** - Azure AD authentication support
- ✅ **TLS/HTTPS** - Encrypted communication enforced

Follows OWASP Top 10 security guidelines. See [Security Best Practices](docs/guides/SecurityBestPractices.md) for details.

---

## 🧪 Testing

The library is thoroughly tested with **114 passing tests** covering:

- Unit tests for all public APIs
- Integration tests with Azure Table Storage
- Security validation tests (injection prevention, sanitization)
- Configuration provider tests
- Exception handling tests

```bash
# Run tests
dotnet test

# With coverage (requires coverlet)
dotnet test --collect:"XPlat Code Coverage"
```

See [Testing Guide](docs/guides/Testing.md) for testing best practices.

---

## 📦 What's Included

### Core Library (`MZ.Logging.AzureTableStorage`)

- `AzureTableStorageLogger` - Main async logger
- `AzureTableStorageLoggerProvider` - ILoggerProvider implementation
- `AzureTableStorageLoggerWrapper` - ILogger adapter
- `ILogStorage` - Storage abstraction
- `AzureTableStorage` - Azure implementation
- `LogEntry`, `LogQuery` - Data models
- `LogValidator` - Input validation
- Custom exceptions and DI extensions

### Configuration Library (`MZ.Logging.Configuration`)

- Azure Key Vault configuration provider
- Azure App Configuration provider
- Configuration model with validation
- Extension methods for easy setup

---

## 🎯 Common Scenarios

### Distributed Tracing

```csharp
// Automatically capture trace IDs from Activity
var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();

logger.LogInformation("Processing request {RequestId}", requestId);
// TraceId is automatically captured from Activity.Current
```

### Structured Logging

```csharp
logger.LogInformation(
    "Order {OrderId} processed for user {UserId} with total {Amount:C}",
    orderId, userId, amount
);
// Structured data is automatically extracted
```

### Exception Logging

```csharp
try
{
    await ProcessOrderAsync(order);
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to process order {OrderId}", order.Id);
    // Exception details and stack trace are stored
}
```

### Querying Logs

```csharp
var logger = serviceProvider.GetRequiredService<AzureTableStorageLogger>();

var query = new LogQuery
{
    PageSize = 100,
    OrderBy = "Timestamp",
    Ascending = false,
    Filters = new Dictionary<string, object>
    {
        { "LogLevel", "Error" },
        { "LoggerName", "MyService" }
    }
};

var (logs, nextToken) = await logger.GetLogsAsync(query);

foreach (var log in logs)
{
    Console.WriteLine($"[{log.Level}] {log.Timestamp}: {log.Message}");
}
```

---

## 🌐 Azure Resources

The library integrates with:

- **Azure Table Storage** - Log data persistence
- **Azure Key Vault** - Secrets management (optional)
- **Azure App Configuration** - Feature flags & settings (optional)
- **Azure Monitor** - OpenTelemetry export (optional)
- **Azure Managed Identity** - Authentication (optional)

### Local Development

Use **Azurite** for local Azure Storage emulation:

```bash
# Install Azurite
npm install -g azurite

# Start Azurite
azurite-table

# Use in code
services.AddAzureTableStorageLogging(
    "UseDevelopmentStorage=true",
    "logs",
    "MyApp"
);
```

---

## 🤝 Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Building from Source

```bash
# Clone repository
git clone https://github.com/masterzdran/mz-logging-azure-tablestorage-csharp.git
cd mz-logging-azure-tablestorage-csharp

# Restore and build
dotnet restore
dotnet build

# Run tests
dotnet test

# Create NuGet packages
dotnet pack -c Release
```

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 Author

**Nuno Cancelo**

- GitHub: [@masterzdran](https://github.com/masterzdran)
- Email: nuno.cancelo@gmail.com

---

## 🙏 Acknowledgments

- Inspired by the Python [MZ.Logging.AzureTableStorage](https://github.com/masterzdran/mz-logging-azure-tablestorage) project
- Built with best practices from Microsoft's logging guidelines
- Security guidance from OWASP Top 10

---

## 📊 Project Stats

- **Lines of Code**: ~3,500+
- **Tests**: 114 (100% passing)
- **Code Coverage**: High (unit + integration tests)
- **Target Frameworks**: .NET 8.0, .NET 7.0
- **Dependencies**: Minimal (Azure SDK, Microsoft.Extensions)
- **Documentation**: 8 comprehensive guides

---

## 🔗 Links

- [NuGet Package](https://www.nuget.org/packages/MZ.Logging.AzureTableStorage)
- [Source Code](https://github.com/masterzdran/mz-logging-azure-tablestorage-csharp)
- [Issue Tracker](https://github.com/masterzdran/mz-logging-azure-tablestorage-csharp/issues)
- [Changelog](CHANGELOG.md)
- [Python Version](https://github.com/masterzdran/mz-logging-azure-tablestorage)

---

**Ready to get started?** Check out the [Quick Start Guide](docs/guides/QuickStart.md)!
