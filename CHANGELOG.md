# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- .NET 10.0 support added to all projects
- Multi-targeting expanded to net10.0;net9.0;net8.0;net7.0

### Planned

- Azure Blob Storage export functionality
- Performance metrics and diagnostics
- Advanced filtering with OData queries
- Batch logging operations
- Log retention policies

## [0.1.0] - 2025-12-23

### Added

- Initial pre-production release with MasterZdran branding
- **BREAKING**: Namespace changed from `MZ.Logging.*` to `MasterZdran.Logging.*`
- **BREAKING**: Package IDs changed to `MasterZdran.Logging.AzureTableStorage` and `MasterZdran.Logging.Configuration`
- Company name updated from "MZ" to "MasterZdran" across all packages
- .NET 9.0 support added alongside .NET 8.0 and .NET 7.0
- Multi-targeting for maximum compatibility (net9.0;net8.0;net7.0)
- Published to NuGet.org for community testing and feedback
- Automatic caller location tracking using CallerFilePath, CallerMemberName, CallerLineNumber attributes
- Location field made optional (nullable) for flexibility
- Full async/await API design
- Azure Table Storage integration
- Microsoft.Extensions.Logging (ILogger, ILogger<T>) support
- OpenTelemetry integration for distributed tracing
- Multiple configuration providers (Key Vault, App Configuration, JSON, Environment)
- OWASP security best practices and input validation
- 113 comprehensive unit and integration tests
- Complete documentation with guides and examples
- NuGet packages:
  - MasterZdran.Logging.AzureTableStorage (core library)
  - MasterZdran.Logging.Configuration (configuration providers)

### Notes

- Version 0.1.0 indicates pre-production status for testing and feedback
- Breaking changes may occur before 1.0.0 stable release
- Production use is not recommended until 1.0.0

## [1.0.0] - Planned

### Added

- Initial release of MasterZdran.Logging.AzureTableStorage for .NET
- Core logging functionality with structured logging
- Azure Table Storage integration
- Async/await API design
- Support for .NET 8.0 and .NET 7.0
- Dependency Injection support (Microsoft.Extensions.DependencyInjection)
- Full Microsoft.Extensions.Logging (ILogger, ILogger<T>) integration
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
- NuGet package support:
  - MasterZdran.Logging.AzureTableStorage (core library)
  - MasterZdran.Logging.Configuration (configuration providers)
- Automated build script (build-nuget.ps1)
- Comprehensive documentation:
  - Quick Start guide
  - Usage guide
  - Configuration guide
  - Security best practices
  - Architecture overview
  - API reference
  - NuGet package guide
- Comprehensive test coverage (114 tests, 100% pass rate)
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

This is a complete rewrite in C# of the [MasterZdran.Logging.AzureTableStorage (Python)](https://github.com/masterzdran/mz-logging-azure-tablestorage) project.

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
