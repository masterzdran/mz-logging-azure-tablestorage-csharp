using MZ.Logging.AzureTableStorage;
using Xunit;
using FluentAssertions;

namespace MZ.Logging.AzureTableStorage.Tests;

/// <summary>
/// Unit tests for LogEntry model validation.
/// </summary>
public class LogEntryTests
{
    [Fact]
    public void LogEntry_ValidEntry_PassesValidation()
    {
        // Arrange
        var logEntry = new LogEntry
        {
            PartitionKey = "TestApp",
            RowKey = "20240101120000000000",
            Level = LogLevel.Information,
            Message = "Test message",
            Timestamp = DateTime.UtcNow.ToString("O"),
            LoggerName = "TestApp",
            Location = "TestFile:42"
        };

        // Act
        var result = logEntry.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void LogEntry_EmptyPartitionKey_FailsValidation()
    {
        // Arrange
        var logEntry = new LogEntry
        {
            PartitionKey = "",
            RowKey = "20240101120000000000",
            Level = LogLevel.Information,
            Message = "Test message",
            Timestamp = DateTime.UtcNow.ToString("O"),
            LoggerName = "TestApp",
            Location = "TestFile:42"
        };

        // Act
        var result = logEntry.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.Contains("PartitionKey"));
    }

    [Fact]
    public void LogEntry_InvalidTimestamp_FailsValidation()
    {
        // Arrange
        var logEntry = new LogEntry
        {
            PartitionKey = "TestApp",
            RowKey = "20240101120000000000",
            Level = LogLevel.Information,
            Message = "Test message",
            Timestamp = "invalid-date",
            LoggerName = "TestApp",
            Location = "TestFile:42"
        };

        // Act
        var result = logEntry.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.Contains("Timestamp"));
    }

    [Fact]
    public void LogEntry_AllFieldsEmpty_FailsValidation()
    {
        // Arrange
        var logEntry = new LogEntry
        {
            PartitionKey = "",
            RowKey = "",
            Message = "",
            LoggerName = "",
            Location = "",
            Timestamp = ""
        };

        // Act
        var result = logEntry.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Count.Should().BeGreaterThan(3);
    }
}

/// <summary>
/// Unit tests for LogQuery model validation.
/// </summary>
public class LogQueryTests
{
    [Fact]
    public void LogQuery_ValidQuery_PassesValidation()
    {
        // Arrange
        var query = new LogQuery
        {
            PageSize = 50,
            OrderBy = "Timestamp",
            Ascending = false,
            Filters = new Dictionary<string, object> { { "LogLevel", "Error" } }
        };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void LogQuery_PageSizeZero_FailsValidation()
    {
        // Arrange
        var query = new LogQuery { PageSize = 0 };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.Contains("PageSize"));
    }

    [Fact]
    public void LogQuery_PageSizeExceedsMax_FailsValidation()
    {
        // Arrange
        var query = new LogQuery { PageSize = 2000 };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.Contains("PageSize"));
    }

    [Fact]
    public void LogQuery_InvalidOrderBy_FailsValidation()
    {
        // Arrange
        var query = new LogQuery { OrderBy = "InvalidField" };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.Contains("OrderBy"));
    }

    [Fact]
    public void LogQuery_ValidOrderByFields_PassValidation()
    {
        // Arrange
        var validFields = new[] { "Timestamp", "LogLevel", "TraceId", "LoggerName", "Location", "Message" };

        // Act & Assert
        foreach (var field in validFields)
        {
            var query = new LogQuery { OrderBy = field };
            var result = query.Validate();
            result.IsValid.Should().BeTrue($"OrderBy field '{field}' should be valid");
        }
    }
}

/// <summary>
/// Unit tests for LogValidator.
/// </summary>
public class LogValidatorTests
{
    [Fact]
    public void Validate_ValidLogEntry_ReturnsValid()
    {
        // Arrange
        var validator = new LogValidator();
        var logEntry = new LogEntry
        {
            PartitionKey = "TestApp",
            RowKey = "20240101120000000000",
            Level = LogLevel.Information,
            Message = "Test message",
            Timestamp = DateTime.UtcNow.ToString("O"),
            LoggerName = "TestApp",
            Location = "TestFile:42"
        };

        // Act
        var result = validator.Validate(logEntry);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NullLogEntry_ThrowsArgumentNullException()
    {
        // Arrange
        var validator = new LogValidator();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => validator.Validate(null!));
    }
}
