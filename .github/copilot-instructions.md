<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# MasterZdran.Logging.AzureTableStorage - C# / .NET

Professional Azure Table Storage logging library for .NET with clean code, OWASP security best practices, and OpenTelemetry integration.

## Project Overview

This is a complete C# rewrite of the Python MasterZdran.Logging.AzureTableStorage project with the following improvements:
- Full .NET 8.0 and .NET 7.0 support
- Async/await native implementation
- Dependency Injection support
- OpenTelemetry integration
- Multiple configuration providers (Key Vault, App Configuration, App Settings)
- OWASP compliance built-in
- Comprehensive documentation

## Technology Stack

- **.NET**: 8.0 (primary), 7.0 (compatibility)
- **Language**: C# 12
- **Testing**: xUnit, Moq, FluentAssertions
- **Azure SDK**: Azure.Data.Tables, Azure.Identity, Azure.Security.KeyVault.Secrets, Azure.Data.AppConfiguration
- **Extensions**: Microsoft.Extensions.DependencyInjection, OpenTelemetry

## Project Structure

```
src/
├── MasterZdran.Logging.AzureTableStorage/
│   ├── Contracts/          - ILogStorage interface
│   ├── Models/            - LogEntry, LogQuery, LogLevel
│   ├── Exceptions/        - Custom exceptions
│   ├── Storage/           - AzureTableStorage implementation
│   ├── Validation/        - ILogValidator, LogValidator
│   ├── Logging/           - AzureTableStorageLogger
│   └── DependencyInjection/  - ServiceCollectionExtensions
└── MasterZdran.Logging.Configuration/
    ├── Models/            - AzureTableStorageLoggingConfiguration
    ├── Providers/         - Key Vault & App Configuration providers
    └── Extensions/        - ConfigurationBuilderExtensions

tests/
├── MasterZdran.Logging.AzureTableStorage.Tests/
└── MasterZdran.Logging.Configuration.Tests/

docs/
├── guides/                - User guides (QuickStart, Configuration, Security)
├── architecture/          - ArchitectureOverview.md
└── api/                  - API Reference

samples/
└── ConsoleApp/           - Sample console application
```

## Key Classes and Interfaces

### Core Logging
- **AzureTableStorageLogger**: Main logger class with async methods (Debug, Information, Warning, Error, Critical)
- **ILogStorage**: Storage abstraction interface
- **AzureTableStorage**: Azure Table Storage implementation with validation and sanitization

### Models
- **LogEntry**: Represents a log entry with validation
- **LogQuery**: Query builder for log retrieval
- **LogLevel**: Enumeration for log levels

### Configuration
- **AzureTableStorageLoggingConfiguration**: Configuration model
- **AzureKeyVaultConfigurationProvider**: Key Vault support
- **AzureAppConfigurationProvider**: App Configuration support

### Validation
- **ILogValidator**: Validation interface
- **LogValidator**: Default implementation
- **ValidationException**: Custom exception for validation errors

## Important Guidelines

### Code Style
- Follow C# coding conventions
- Use nullable reference types (CS8618, CS8601)
- Add XML documentation to public APIs
- Maximum line length: 120 characters
- Use async/await for all I/O operations

### Security (OWASP)
- All inputs validated before processing
- OData filter injection prevention (escape single quotes)
- Connection string validation
- Secure exception handling (no PII in messages)
- Azure Key Vault for secrets
- Support for Managed Identity

### Testing
- xUnit test framework
- Moq for mocking
- FluentAssertions for readability
- Test naming: `MethodName_Scenario_ExpectedResult`
- All public APIs must have tests

### Documentation
- Keep README.md updated
- Add examples for new features
- Update CHANGELOG.md for releases
- Maintain guides in /docs/guides/
- Add architecture documentation for major changes

## Build Commands

```bash
# Build
dotnet build
dotnet build -c Release

# Test
dotnet test
dotnet test --logger "console;verbosity=detailed"

# Publish
dotnet publish -c Release

# Pack NuGet
dotnet pack -c Release
```

## Common Patterns

### Dependency Injection Setup
```csharp
services.AddAzureTableStorageLogging(
    connectionString,
    tableName,
    loggerName
);
var logger = serviceProvider.GetRequiredService<AzureTableStorageLogger>();
```

### Logging with Metadata
```csharp
await logger.InformationAsync(
    "Operation completed",
    traceId: traceId,
    metadata: new Dictionary<string, object> { { "key", "value" } }
);
```

### Exception Handling
```csharp
catch (ValidationException ex)
{
    await logger.ErrorAsync("Validation failed", exception: ex);
}
catch (StorageException ex)
{
    await logger.CriticalAsync("Storage error", exception: ex);
}
```

### Configuration Loading
```csharp
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddAzureKeyVault(keyVaultUri)
    .AddAzureAppConfiguration(appConfigConnection)
    .Build();
var loggingConfig = config.GetAzureTableStorageLoggingConfiguration();
```

## File Organization

- **Contracts/**: Define interfaces and contracts
- **Models/**: Data models and value objects
- **Exceptions/**: Custom exception types
- **Storage/**: Storage implementations and providers
- **Validation/**: Validation logic
- **Logging/**: Logger implementations
- **DependencyInjection/**: DI registration extensions
- **docs/**: User-facing documentation
- **tests/**: Unit and integration tests
- **samples/**: Example applications

## Testing Patterns

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var mockStorage = new Mock<ILogStorage>();
    var logger = new AzureTableStorageLogger(mockStorage.Object, "TestApp");

    // Act
    await logger.InformationAsync("Test");

    // Assert
    mockStorage.Verify(x => x.StoreLogAsync(...), Times.Once);
}
```

## Documentation Requirements

- Update /docs/README.md for major changes
- Keep guides current in /docs/guides/
- Add code examples for new features
- Update CHANGELOG.md before release
- XML comments on all public members

## Performance Considerations

- Async operations for all I/O
- Minimal allocations in hot paths
- Pagination for large result sets
- Connection pooling via Azure SDK
- Cancellation token support

## Security Checklist

- [ ] All inputs validated
- [ ] No sensitive data in logs
- [ ] Connection strings not hardcoded
- [ ] HTTPS/TLS enforced
- [ ] Error messages sanitized
- [ ] Tests for injection prevention
- [ ] Azure Key Vault integration
- [ ] Managed Identity support

## Release Process

1. Update CHANGELOG.md
2. Bump version in .csproj files
3. Create release notes
4. Tag commit with version
5. Build and publish NuGet packages
6. Update documentation

## Useful Resources

- [Azure SDK for .NET Documentation](https://learn.microsoft.com/en-us/dotnet/azure/)
- [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [xUnit Documentation](https://xunit.net/)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)
