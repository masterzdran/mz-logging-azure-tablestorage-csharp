# MasterZdran.Logging.AzureTableStorage Documentation

Complete documentation index for the MasterZdran.Logging.AzureTableStorage .NET library.

---

## 📚 Documentation Structure

### Getting Started

- **[README.md](README.md)** - Project overview, quick start, and key features
- **[Quick Start Guide](docs/guides/QuickStart.md)** - 5-minute setup guide
- **[Usage Guide](docs/guides/UsageGuide.md)** - Comprehensive usage scenarios and examples
- **[NuGet Package Guide](NUGET.md)** - Building, testing, and publishing NuGet packages

### Configuration

- **[Configuration Guide](docs/guides/Configuration.md)** - All configuration options and sources
  - appsettings.json
  - Azure Key Vault
  - Azure App Configuration
  - Environment variables

### API Documentation

- **[API Reference](docs/api/ApiReference.md)** - Complete API documentation
  - Core logging classes
  - Storage abstractions
  - Models and validation
  - Configuration helpers
  - Dependency injection
  - Exception hierarchy

### Architecture & Design

- **[Architecture Overview](docs/architecture/ArchitectureOverview.md)** - System design and patterns
  - Clean Architecture principles
  - SOLID design patterns
  - Layer responsibilities
  - Data flow diagrams

### Security

- **[Security Best Practices](docs/guides/SecurityBestPractices.md)** - OWASP compliance guide
  - Input validation
  - Injection prevention
  - Secrets management
  - Azure security integration

### Testing

- **[Testing Guide](docs/guides/Testing.md)** - Testing strategies and best practices
  - Unit testing patterns
  - Integration testing
  - Mocking strategies
  - Test coverage

### Project Information

- **[CHANGELOG.md](CHANGELOG.md)** - Version history and release notes
- **[CONTRIBUTING.md](CONTRIBUTING.md)** - Contribution guidelines
- **[LICENSE](LICENSE)** - MIT License

---

## 🚀 Quick Navigation

### For New Users

1. Start with [README.md](README.md) for project overview
2. Follow [Quick Start Guide](docs/guides/QuickStart.md) to get running in 5 minutes
3. Explore [Usage Guide](docs/guides/UsageGuide.md) for your specific scenario

### For Developers

1. Review [API Reference](docs/api/ApiReference.md) for detailed API documentation
2. Check [Architecture Overview](docs/architecture/ArchitectureOverview.md) for design patterns
3. Follow [Security Best Practices](docs/guides/SecurityBestPractices.md) for secure implementation

### For Contributors

1. Read [CONTRIBUTING.md](CONTRIBUTING.md) for contribution guidelines
2. Review [Testing Guide](docs/guides/Testing.md) for testing requirements
3. Check [CHANGELOG.md](CHANGELOG.md) for version history

---

## 📖 Documentation Overview

### [README.md](README.md)

Main project documentation with quick start, features, and examples.

**Topics Covered:**

- Installation
- Basic usage with Microsoft.Extensions.Logging
- ASP.NET Core integration
- Console application examples
- Architecture overview
- Configuration options
- Security features
- Testing information

---

### [docs/guides/QuickStart.md](docs/guides/QuickStart.md)

Get up and running in 5 minutes.

**Topics Covered:**

- Prerequisites
- Installation steps
- Basic configuration
- First log entry
- Querying logs
- Next steps

---

### [docs/guides/UsageGuide.md](docs/guides/UsageGuide.md)

Comprehensive usage scenarios with practical examples.

**Topics Covered:**

- ASP.NET Core integration (controllers, middleware)
- Console applications
- Worker services
- Azure Functions
- Structured logging patterns
- Exception handling
- Distributed tracing
- Querying logs
- Performance optimization
- Local development with Azurite
- Production deployment

---

### [docs/guides/Configuration.md](docs/guides/Configuration.md)

Complete configuration reference.

**Topics Covered:**

- Configuration model
- appsettings.json setup
- Azure Key Vault integration
- Azure App Configuration
- Environment variables
- Managed Identity
- Configuration validation

---

### [docs/api/ApiReference.md](docs/api/ApiReference.md)

Complete API documentation for all public classes and interfaces.

**Topics Covered:**

- AzureTableStorageLogger (all methods)
- ILogger integration
- ILogStorage interface
- AzureTableStorage implementation
- LogEntry, LogQuery models
- Configuration helpers
- Validation interfaces
- Dependency injection
- Exception types
- Security considerations
- Performance tips

---

### [docs/architecture/ArchitectureOverview.md](docs/architecture/ArchitectureOverview.md)

Deep dive into system architecture and design decisions.

**Topics Covered:**

- Clean Architecture implementation
- SOLID principles
- Layer separation
- Design patterns used
- Data flow
- Security architecture
- Performance considerations
- Testability

---

### [docs/guides/SecurityBestPractices.md](docs/guides/SecurityBestPractices.md)

OWASP-compliant security guidelines.

**Topics Covered:**

- Input validation
- OData injection prevention
- Sanitization strategies
- Secrets management (Key Vault)
- Managed Identity
- Secure exception handling
- TLS/HTTPS enforcement
- Security testing

---

### [docs/guides/Testing.md](docs/guides/Testing.md)

Testing strategies and best practices.

**Topics Covered:**

- Unit testing patterns
- Integration testing
- Mocking with Moq
- Test data builders
- Coverage requirements
- CI/CD integration
- Test organization

---

## 🎯 Common Use Cases

### Scenario 1: Adding to Existing ASP.NET Core App

1. Read [Quick Start Guide](docs/guides/QuickStart.md)
2. Follow ASP.NET Core section in [Usage Guide](docs/guides/UsageGuide.md)
3. Configure via [Configuration Guide](docs/guides/Configuration.md)

### Scenario 2: Building New Console Application

1. Read [Quick Start Guide](docs/guides/QuickStart.md)
2. Follow Console Application section in [Usage Guide](docs/guides/UsageGuide.md)
3. Review [API Reference](docs/api/ApiReference.md) for advanced features

### Scenario 3: Implementing Distributed Tracing

1. Review distributed tracing in [Usage Guide](docs/guides/UsageGuide.md)
2. Check [API Reference](docs/api/ApiReference.md) for trace ID parameters
3. See [Architecture Overview](docs/architecture/ArchitectureOverview.md) for patterns

### Scenario 4: Securing Production Deployment

1. Read [Security Best Practices](docs/guides/SecurityBestPractices.md)
2. Configure Key Vault via [Configuration Guide](docs/guides/Configuration.md)
3. Review production section in [Usage Guide](docs/guides/UsageGuide.md)

### Scenario 5: Contributing to the Project

1. Read [CONTRIBUTING.md](CONTRIBUTING.md)
2. Review [Testing Guide](docs/guides/Testing.md)
3. Check [Architecture Overview](docs/architecture/ArchitectureOverview.md)

---

## 📊 Documentation Statistics

| Category        | Files | Lines      | Status          |
| --------------- | ----- | ---------- | --------------- |
| Getting Started | 3     | ~2,000     | ✅ Complete     |
| Configuration   | 1     | ~500       | ✅ Complete     |
| API Reference   | 1     | ~1,200     | ✅ Complete     |
| Architecture    | 1     | ~800       | ✅ Complete     |
| Security        | 1     | ~600       | ✅ Complete     |
| Testing         | 1     | ~400       | ✅ Complete     |
| **Total**       | **8** | **~5,500** | **✅ Complete** |

---

## 🔍 Search Tips

### Finding Specific Topics

**Configuration:**

- Search "appsettings" in [Configuration Guide](docs/guides/Configuration.md)
- Search "Key Vault" in [Configuration Guide](docs/guides/Configuration.md)

**API Methods:**

- Search "InformationAsync" in [API Reference](docs/api/ApiReference.md)
- Search "GetLogsAsync" in [API Reference](docs/api/ApiReference.md)

**Security:**

- Search "injection" in [Security Best Practices](docs/guides/SecurityBestPractices.md)
- Search "validation" in [API Reference](docs/api/ApiReference.md)

**Examples:**

- Search "ASP.NET" in [Usage Guide](docs/guides/UsageGuide.md)
- Search "Azure Functions" in [Usage Guide](docs/guides/UsageGuide.md)

---

## 📝 Documentation Standards

All documentation follows these standards:

- ✅ **Markdown format** - Clean, readable markdown
- ✅ **Code examples** - Runnable, tested code snippets
- ✅ **Clear headings** - Hierarchical structure
- ✅ **Cross-references** - Links between related topics
- ✅ **Up-to-date** - Synchronized with codebase
- ✅ **Comprehensive** - Covers all features
- ✅ **Accessible** - Clear language, minimal jargon

---

## 🔄 Documentation Maintenance

### Keeping Documentation Updated

When making code changes:

1. Update [API Reference](docs/api/ApiReference.md) for API changes
2. Add examples to [Usage Guide](docs/guides/UsageGuide.md) for new features
3. Update [CHANGELOG.md](CHANGELOG.md) with changes
4. Review [README.md](README.md) for feature additions

### Version Documentation

Each release includes:

- Updated [CHANGELOG.md](CHANGELOG.md)
- API reference review
- Example validation
- Security review

---

## 🆘 Getting Help

### Documentation Issues

If you find issues with documentation:

1. Check [GitHub Issues](https://github.com/masterzdran/mz-logging-azure-tablestorage-csharp/issues)
2. Search existing documentation
3. Open a new issue with "docs:" prefix

### Feature Requests

For documentation requests:

1. Review existing documentation first
2. Open GitHub issue with "docs:" prefix
3. Describe what's missing or unclear

---

## 📧 Contact

**Author:** Nuno Cancelo

- **Email:** nuno.cancelo@gmail.com
- **GitHub:** [@masterzdran](https://github.com/masterzdran)

---

## 📜 License

All documentation is licensed under [MIT License](LICENSE).

---

**Last Updated:** December 23, 2024  
**Documentation Version:** 1.0.0  
**Library Version:** 1.0.0
