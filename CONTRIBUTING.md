# Contributing to MasterZdran.Logging.AzureTableStorage

Thank you for your interest in contributing! This document provides guidelines and instructions for contributing.

## Code of Conduct

This project adheres to the Contributor Covenant Code of Conduct. By participating, you are expected to uphold this code.

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- Visual Studio Code or Visual Studio 2022
- Git

### Setup Development Environment

1. Fork the repository
2. Clone your fork:
   ```bash
   git clone https://github.com/YOUR_USERNAME/mz-logging-azure-tablestorage-csharp.git
   cd mz-logging-azure-tablestorage-csharp
   ```
3. Add upstream remote:
   ```bash
   git remote add upstream https://github.com/masterzdran/mz-logging-azure-tablestorage-csharp.git
   ```
4. Create a branch for your feature:
   ```bash
   git checkout -b feature/your-feature-name
   ```

## Development Guidelines

### Code Style

- Follow [C# coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Keep methods focused and single-responsibility
- Add XML documentation comments for public APIs

### Formatting

```csharp
// Good: Clear, documented code
/// <summary>
/// Validates the log entry before storage.
/// </summary>
/// <param name="logEntry">The log entry to validate.</param>
/// <returns>Validation result with any errors.</returns>
public ValidationResult Validate(LogEntry logEntry)
{
    ArgumentNullException.ThrowIfNull(logEntry);
    return logEntry.Validate();
}

// Bad: Undocumented, unclear
public void Validate(LogEntry entry)
{
    var result = entry.Validate();
}
```

### Testing

All new features must include tests:

```csharp
[Fact]
public async Task InformationAsync_ValidMessage_StoresLog()
{
    // Arrange
    var mockStorage = new Mock<ILogStorage>();
    var logger = new AzureTableStorageLogger(mockStorage.Object, "TestApp");
    var message = "Test message";

    // Act
    await logger.InformationAsync(message);

    // Assert
    mockStorage.Verify(s => s.StoreLogAsync(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.Is<LogEntry>(e => e.Message == message),
        It.IsAny<CancellationToken>()), Times.Once);
}
```

### Documentation

- Update relevant documentation files in `/docs`
- Add XML comments to public APIs
- Include examples for new features
- Update CHANGELOG.md

## Submitting Changes

### Before Committing

1. Run tests:

   ```bash
   dotnet test
   ```

2. Check code quality:

   ```bash
   dotnet build -c Release
   ```

3. Update documentation if needed

### Commit Message Guidelines

```
[TYPE] Brief description (50 chars max)

Detailed explanation of changes (if needed).
- Bullet point 1
- Bullet point 2

Fixes #123
```

**Types:**

- `feat` - New feature
- `fix` - Bug fix
- `docs` - Documentation
- `test` - Test additions/changes
- `refactor` - Code refactoring
- `perf` - Performance improvements
- `security` - Security fixes

### Example Commits

```
feat: Add log export to Azure Blob Storage

- Implements ILogExporter interface
- Adds AzureBlobStorageExporter class
- Includes configuration options
- Adds comprehensive tests

Closes #456
```

```
fix: Prevent OData injection in filter values

- Properly escape single quotes in filter values
- Add security tests for injection prevention
- Document secure filtering practices

Fixes #123
```

## Pull Request Process

1. **Update** your branch with latest main:

   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Push** to your fork:

   ```bash
   git push origin feature/your-feature-name
   ```

3. **Create** Pull Request on GitHub with:

   - Clear title describing the change
   - Description of what changed and why
   - Reference to related issues (`Fixes #123`)
   - Evidence of testing
   - Screenshots if UI changes

4. **Address** review comments:
   ```bash
   git commit --amend
   git push origin feature/your-feature-name --force
   ```

## Pull Request Checklist

- [ ] Code follows style guidelines
- [ ] Self-review of code completed
- [ ] Comments added for complex logic
- [ ] Documentation updated
- [ ] Tests added/updated
- [ ] All tests pass locally
- [ ] No breaking changes (or documented)
- [ ] Changelog updated

## Reporting Issues

### Security Issues

**IMPORTANT:** Do not create public issues for security vulnerabilities.
Email security@example.com with details.

### Bug Reports

Include:

- .NET version
- OS (Windows/Linux/macOS)
- Reproduction steps
- Expected vs. actual behavior
- Relevant logs/error messages
- Code snippet if applicable

### Feature Requests

Include:

- Use case description
- Proposed API/implementation (optional)
- Related issues or PRs
- Alternatives considered

## Areas for Contribution

### High Priority

- [ ] Batch logging operations
- [ ] Performance optimizations
- [ ] Additional configuration providers
- [ ] Azure Blob Storage export

### Medium Priority

- [ ] Advanced filtering with expressions
- [ ] Log retention policies
- [ ] Custom formatters
- [ ] Performance metrics

### Low Priority

- [ ] Additional examples
- [ ] Documentation improvements
- [ ] Test coverage expansion
- [ ] CI/CD enhancements

## Questions?

- Check [Documentation](/docs/README.md)
- Search [GitHub Issues](https://github.com/masterzdran/mz-logging-azure-tablestorage-csharp/issues)
- Email: nuno.cancelo@gmail.com

## License

By contributing to this project, you agree that your contributions will be licensed under the MIT License.

Thank you for contributing! 🎉
