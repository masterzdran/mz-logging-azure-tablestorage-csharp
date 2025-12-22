# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned

- Azure Blob Storage export functionality
- Performance metrics and diagnostics
- Advanced filtering with OData queries
- Batch logging operations
- Log retention policies

## [1.0.0] - 2024-12-21

### Added

- Initial release of MZ.Logging.AzureTableStorage for .NET
- Core logging functionality with structured logging
- Azure Table Storage integration
- Async/await API design
- Support for .NET 8.0 and .NET 7.0
- Dependency Injection support (Microsoft.Extensions.DependencyInjection)
- OpenTelemetry integration for distributed tracing
- Multiple configuration providers:
  - Azure Key Vault
  - Azure App Configuration
  - JSON configuration files
  - Environment variables
- Input validation and sanitization
- OWASP security best practices implementation
- Custom exception types (LoggingException, StorageException, ValidationException)
- Log retrieval with pagination and filtering
- Automatic caller location tracking (file, method, line number)
- Distributed trace ID support
- Comprehensive documentation:
  - Quick Start guide
  - Configuration guide
  - Security best practices
  - Architecture overview
- Comprehensive test coverage
- Cross-platform support (Windows, Linux, macOS)

### Features

- `AzureTableStorageLogger` - Main logging class with methods:
  - `DebugAsync()`
  - `InformationAsync()`
  - `WarningAsync()`
  - `ErrorAsync()`
  - `CriticalAsync()`
  - `GetLogsAsync()`
  - `GetLogEntryAsync()`
- `AzureTableStorage` - Storage implementation with:
  - Automatic table creation
  - Input sanitization
  - OData filter building with injection prevention
  - Pagination support
  - Comprehensive error handling
- `ILogStorage` - Storage abstraction for extensibility
- `LogEntry` - Structured log entry model
- `LogQuery` - Query builder for log retrieval
- `AzureTableStorageLoggingConfiguration` - Configuration model
- `ServiceCollectionExtensions` - DI registration helpers
- `ConfigurationBuilderExtensions` - Configuration provider registration

### Documentation

- README.md - Project overview
- /docs/guides/QuickStart.md - 5-minute setup guide
- /docs/guides/Configuration.md - Configuration options and examples
- /docs/guides/SecurityBestPractices.md - OWASP guidelines and patterns
- /docs/architecture/ArchitectureOverview.md - System design and components

### Security

- Input validation on all APIs
- SQL/OData injection prevention
- PII protection guidance
- Secure exception handling
- Integration with Azure Key Vault
- Support for Managed Identity authentication
- HTTPS/TLS enforcement

### Testing

- Unit tests for core functionality
- Integration tests with Azure Storage
- Configuration provider tests
- Security validation tests
- xUnit test framework
- Moq for mocking
- FluentAssertions for test readability

### Build & Deployment

- Solution file with 4 projects
- NuGet package metadata configured
- XML documentation generation enabled
- Support for both net8.0 and net7.0 target frameworks
- CI/CD ready structure

## Version History

### From Python Version

This is a complete rewrite in C# of the [MZ.Logging.AzureTableStorage (Python)](https://github.com/masterzdran/mz-logging-azure-tablestorage) project.

**Key Improvements:**

- Compile-time type checking
- Better performance through native compilation
- Enhanced security through CLR runtime
- Better enterprise integration
- Synchronous and asynchronous APIs
- Native async/await support (not asyncio)
- Built-in dependency injection
- OpenTelemetry integration
- Multiple configuration sources
- Comprehensive documentation

---

**Repository:** https://github.com/masterzdran/mz-logging-azure-tablestorage-csharp  
**License:** MIT  
**Author:** Nuno Cancelo
