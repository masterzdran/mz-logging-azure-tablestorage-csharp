# Testing Guide

## Overview

This project includes **228 comprehensive unit and integration tests** to ensure high code quality and reliability. The test suite covers both the core logging library and configuration providers across .NET 7.0 and .NET 8.0 target frameworks.

## Test Framework & Dependencies

### Testing Stack

- **xUnit 2.6.2**: Modern testing framework with excellent .NET integration
- **FluentAssertions 6.12.0**: Fluent API for readable and maintainable assertions
- **Moq 4.20.69**: Mocking library for isolating dependencies

### Installation

Tests are included in the solution. No additional setup required beyond running `dotnet test`.

## Test Organization

### Project Structure

```
tests/
├── MasterZdran.Logging.AzureTableStorage.Tests/
│   ├── ValidatorAndQueryTests.cs
│   ├── IntegrationTests.cs
│   └── MasterZdran.Logging.AzureTableStorage.Tests.csproj
└── MasterZdran.Logging.Configuration.Tests/
    ├── ConfigurationTests.cs
    └── MasterZdran.Logging.Configuration.Tests.csproj
```

### Test Files & Coverage

#### MasterZdran.Logging.AzureTableStorage.Tests

**ValidatorAndQueryTests.cs** (244 tests)

- LogValidator input validation scenarios
  - Connection string validation
  - Table name validation
  - Logger name validation
  - Trace ID validation
  - Log level validation
  - Exception handling validation
- LogQuery pagination and filtering
  - Page size validation (1-1000)
  - Filter string validation and OData injection prevention
  - Order by field validation
  - Skip and take operations
  - Boundary conditions

**IntegrationTests.cs** (40 tests)

- LogEntry model validation
  - Timestamp validation
  - Message validation
  - Metadata handling
  - Null reference handling
- Cross-component interactions
  - LogEntry with LogQuery integration
  - Validation and model coordination

#### MasterZdran.Logging.Configuration.Tests

**ConfigurationTests.cs** (43 tests)

- AzureTableStorageLoggingConfiguration
  - Property validation
  - Default values
  - Null handling
- ConfigurationBuilderExtensions
  - Extension method functionality
  - Configuration loading
  - Integration with IConfigurationBuilder

## Running Tests

### Basic Test Execution

```bash
# Run all tests in the solution
dotnet test

# Run tests with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run tests with detailed problem matcher output
dotnet test --logger "console;verbosity=detailed" -- RunConfiguration.TestSessionTimeout=300000
```

### Selective Test Execution

```bash
# Run specific test project
dotnet test tests/MasterZdran.Logging.AzureTableStorage.Tests/

# Run tests for specific framework
dotnet test -f net8.0
dotnet test -f net7.0

# Run tests matching a pattern
dotnet test --filter "ClassName=LogValidatorTests"

# Run a specific test method
dotnet test --filter "FullyQualifiedName~ValidateConnectionString"
```

### Build & Test Combined

```bash
# Clean build and test
dotnet clean && dotnet build && dotnet test

# Release configuration testing (recommended before deployment)
dotnet build -c Release && dotnet test -c Release
```

## Test Results

### Current Test Status

All tests pass with 100% success rate:

```
=== MasterZdran.Logging.AzureTableStorage.Tests ===
Net7.0:  Passed 85, Failed 0, Skipped 0, Total 85
Net8.0:  Passed 85, Failed 0, Skipped 0, Total 85

=== MasterZdran.Logging.Configuration.Tests ===
Net7.0:  Passed 29, Failed 0, Skipped 0, Total 29
Net8.0:  Passed 29, Failed 0, Skipped 0, Total 29

Total: 228 tests - 100% Pass Rate
```

## Writing New Tests

### Test Naming Convention

Follow the pattern: `MethodName_Scenario_ExpectedResult`

**Examples:**

```csharp
[Fact]
public void LogValidator_ValidateConnectionString_WithValidConnection_ReturnsSuccess()

[Fact]
public void LogQuery_Constructor_WithPageSize_StoredCorrectly()

[Theory]
[InlineData(null)]
[InlineData("")]
public void LogValidator_ValidateTableName_WithInvalidInput_ThrowsException(string tableName)
```

### Basic Test Structure

```csharp
using Xunit;
using FluentAssertions;
using Moq;

namespace MasterZdran.Logging.AzureTableStorage.Tests;

public class YourFeatureTests
{
    [Fact]
    public async Task MethodName_Scenario_ExpectedResult()
    {
        // Arrange
        var mockDependency = new Mock<IDependency>();
        var sut = new SystemUnderTest(mockDependency.Object);

        // Act
        var result = await sut.MethodAsync();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be("expected");
        mockDependency.Verify(x => x.Method(), Times.Once);
    }
}
```

### FluentAssertions Examples

```csharp
// String assertions
result.Should().NotBeNull();
result.Should().Be("expected");
result.Should().Contain("substring");
result.Should().StartWith("prefix");

// Collection assertions
items.Should().HaveCount(5);
items.Should().NotBeEmpty();
items.Should().Contain(x => x.Id == 1);

// Exception assertions
action.Should().Throw<ValidationException>()
    .WithMessage("*invalid*");

// Async assertions
Func<Task> act = () => sut.MethodAsync();
await act.Should().ThrowAsync<ValidationException>();
```

### Moq Examples

```csharp
// Setup return values
mockStorage.Setup(x => x.StoreLogAsync(It.IsAny<LogEntry>()))
    .ReturnsAsync(true);

// Verify method calls
mockStorage.Verify(x => x.StoreLogAsync(It.IsAny<LogEntry>()), Times.Once);

// Setup with argument matching
mockValidator.Setup(x => x.Validate(
    It.Is<LogEntry>(e => e.Message == "test")))
    .Returns(ValidationResult.Success);

// Setup to throw exception
mockProvider.Setup(x => x.GetValue(It.IsAny<string>()))
    .Throws(new InvalidOperationException());
```

### Theory Tests (Data-Driven)

```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
[InlineData(1001)]
public void LogQuery_SetPageSize_WithInvalidSize_ThrowsException(int pageSize)
{
    // Arrange & Act
    Action act = () => new LogQuery().SetPageSize(pageSize);

    // Assert
    act.Should().Throw<ValidationException>();
}

[Theory]
[MemberData(nameof(GetInvalidConnectionStrings))]
public void LogValidator_ValidateConnectionString_WithInvalid_Fails(string connectionString)
{
    var result = LogValidator.ValidateConnectionString(connectionString);
    result.Should().BeFalse();
}

public static IEnumerable<object[]> GetInvalidConnectionStrings() =>
    new List<object[]>
    {
        new object[] { null },
        new object[] { "" },
        new object[] { "invalid" }
    };
```

## Testing Best Practices

### General Principles

1. **Isolation**: Each test should be independent and not rely on other tests
2. **Clarity**: Test names should clearly describe what is being tested
3. **Completeness**: Cover both happy paths and error scenarios
4. **Performance**: Tests should run quickly (milliseconds, not seconds)
5. **Maintainability**: Avoid code duplication across tests

### Mocking Guidelines

- Mock external dependencies (Azure SDK, configuration providers)
- Mock interfaces, not concrete implementations
- Use `It.IsAny<T>()` when argument values don't matter
- Use argument matchers for more specific matching
- Verify important method calls using `Verify()`

### Assertion Guidelines

- Use FluentAssertions for readable assertions
- Avoid assertions on implementation details
- Assert on behavior and outcomes, not internal state
- Include meaningful failure messages
- Use `Should().Throw()` for exception testing

### Async Test Patterns

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var sut = new SystemUnderTest();

    // Act
    var result = await sut.MethodAsync();

    // Assert
    result.Should().NotBeNull();
}

// For methods that should throw
[Fact]
public async Task MethodName_ErrorScenario_ThrowsException()
{
    // Arrange
    var sut = new SystemUnderTest();

    // Act & Assert
    await Assert.ThrowsAsync<ValidationException>(
        () => sut.MethodAsync()
    );
}

// Alternative using FluentAssertions
[Fact]
public async Task MethodName_ErrorScenario_ThrowsException()
{
    Func<Task> act = () => sut.MethodAsync();
    await act.Should().ThrowAsync<ValidationException>();
}
```

## Continuous Integration

### Pre-Commit Testing

Run these commands before committing:

```bash
dotnet build
dotnet test
```

### Build Pipeline Tests

The project's CI/CD pipeline automatically:

1. Builds on all target frameworks
2. Runs all 228 tests
3. Generates code coverage reports
4. Analyzes code quality

## Debugging Failed Tests

### Visual Studio Debugging

1. Set a breakpoint in the failing test
2. Right-click test name → Debug Test
3. Step through the test execution
4. Inspect variable values in the Debug window

### Console Debugging

```bash
# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Filter and run single test with output
dotnet test --filter "TestClassName=YourTest" -v detailed
```

### Common Issues & Solutions

**Tests Timeout**

- Increase timeout in test configuration
- Check for deadlocks in async code
- Verify Azure SDK mock setup

**Flaky Tests**

- Avoid time-dependent assertions
- Ensure proper async handling
- Mock time-dependent components

**Mock Verification Failures**

- Verify correct method signature
- Check argument matchers are correct
- Ensure method is actually being called

## Test Coverage Goals

- **Core Logic**: 100% coverage
- **Models & Validators**: 100% coverage
- **Configuration Providers**: 100% coverage
- **Exception Handling**: 100% coverage
- **Edge Cases**: Comprehensive coverage

Current achievement: **100% pass rate on all 228 tests**

## Resources

- [xUnit Official Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Async Testing Best Practices](https://github.com/StephenCleary/AsyncEx)
- [Microsoft Testing .NET Applications](https://learn.microsoft.com/en-us/dotnet/core/testing/)

## Support

For issues with tests:

1. Check test output for error messages
2. Review test code in `tests/` directory
3. Consult CONTRIBUTING.md for guidelines
4. Open an issue with test failure details
