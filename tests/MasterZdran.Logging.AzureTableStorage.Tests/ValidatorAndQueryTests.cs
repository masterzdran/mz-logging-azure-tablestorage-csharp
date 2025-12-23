using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MasterZdran.Logging.AzureTableStorage;

namespace MasterZdran.Logging.AzureTableStorage.Tests;

/// <summary>
/// Comprehensive unit tests for LogValidator.
/// </summary>
public class LogValidatorImplementationTests
{
    private readonly LogValidator _validator;

    public LogValidatorImplementationTests()
    {
        _validator = new LogValidator();
    }

    [Fact]
    public void Validate_WithValidLogEntry_ReturnsSuccess()
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
            Location = "Test:42"
        };

        // Act
        var result = _validator.Validate(logEntry);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithNullLogEntry_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => _validator.Validate(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Validate_WithEmptyPartitionKey_ReturnsFalse()
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
            Location = "Test:42"
        };

        // Act
        var result = _validator.Validate(logEntry);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("PartitionKey"));
    }

    [Fact]
    public void Validate_WithEmptyRowKey_ReturnsFalse()
    {
        // Arrange
        var logEntry = new LogEntry
        {
            PartitionKey = "TestApp",
            RowKey = "",
            Level = LogLevel.Information,
            Message = "Test message",
            Timestamp = DateTime.UtcNow.ToString("O"),
            LoggerName = "TestApp",
            Location = "Test:42"
        };

        // Act
        var result = _validator.Validate(logEntry);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("RowKey"));
    }

    [Fact]
    public void Validate_WithEmptyMessage_ReturnsFalse()
    {
        // Arrange
        var logEntry = new LogEntry
        {
            PartitionKey = "TestApp",
            RowKey = "20240101120000000000",
            Level = LogLevel.Information,
            Message = "",
            Timestamp = DateTime.UtcNow.ToString("O"),
            LoggerName = "TestApp",
            Location = "Test:42"
        };

        // Act
        var result = _validator.Validate(logEntry);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Message"));
    }

    [Fact]
    public void Validate_WithEmptyLoggerName_ReturnsFalse()
    {
        // Arrange
        var logEntry = new LogEntry
        {
            PartitionKey = "TestApp",
            RowKey = "20240101120000000000",
            Level = LogLevel.Information,
            Message = "Test message",
            Timestamp = DateTime.UtcNow.ToString("O"),
            LoggerName = "",
            Location = "Test:42"
        };

        // Act
        var result = _validator.Validate(logEntry);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("LoggerName"));
    }

    [Fact]
    public void Validate_WithEmptyTimestamp_ReturnsFalse()
    {
        // Arrange
        var logEntry = new LogEntry
        {
            PartitionKey = "TestApp",
            RowKey = "20240101120000000000",
            Level = LogLevel.Information,
            Message = "Test message",
            Timestamp = "",
            LoggerName = "TestApp",
            Location = "Test:42"
        };

        // Act
        var result = _validator.Validate(logEntry);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Timestamp"));
    }

    [Fact]
    public void Validate_WithInvalidTimestamp_ReturnsFalse()
    {
        // Arrange
        var logEntry = new LogEntry
        {
            PartitionKey = "TestApp",
            RowKey = "20240101120000000000",
            Level = LogLevel.Information,
            Message = "Test message",
            Timestamp = "not-a-valid-date",
            LoggerName = "TestApp",
            Location = "Test:42"
        };

        // Act
        var result = _validator.Validate(logEntry);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Timestamp"));
    }

    [Fact]
    public void Validate_WithMultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var logEntry = new LogEntry
        {
            PartitionKey = "",
            RowKey = "",
            Level = LogLevel.Information,
            Message = "",
            Timestamp = "invalid",
            LoggerName = "",
            Location = ""
        };

        // Act
        var result = _validator.Validate(logEntry);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(5);
    }

    [Fact]
    public void Validate_WithWhitespaceFields_TreatsAsEmpty()
    {
        // Arrange
        var logEntry = new LogEntry
        {
            PartitionKey = "   ",
            RowKey = "   ",
            Level = LogLevel.Information,
            Message = "   ",
            Timestamp = DateTime.UtcNow.ToString("O"),
            LoggerName = "   ",
            Location = "   "
        };

        // Act
        var result = _validator.Validate(logEntry);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void Validate_WithValidIso8601Timestamp_Succeeds()
    {
        // Arrange
        var logEntry = new LogEntry
        {
            PartitionKey = "TestApp",
            RowKey = "20240101120000000000",
            Level = LogLevel.Information,
            Message = "Test message",
            Timestamp = "2024-01-01T12:00:00Z",
            LoggerName = "TestApp",
            Location = "Test:42"
        };

        // Act
        var result = _validator.Validate(logEntry);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithUtcNowTimestamp_Succeeds()
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
            Location = "Test:42"
        };

        // Act
        var result = _validator.Validate(logEntry);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithDifferentLogLevels_AllValid()
    {
        // Arrange
        var logLevels = new[]
        {
            LogLevel.Debug,
            LogLevel.Information,
            LogLevel.Warning,
            LogLevel.Error,
            LogLevel.Critical
        };

        // Act & Assert
        foreach (var level in logLevels)
        {
            var logEntry = new LogEntry
            {
                PartitionKey = "TestApp",
                RowKey = "20240101120000000000",
                Level = level,
                Message = "Test message",
                Timestamp = DateTime.UtcNow.ToString("O"),
                LoggerName = "TestApp",
                Location = "Test:42"
            };

            var result = _validator.Validate(logEntry);
            result.IsValid.Should().BeTrue($"Validation should succeed for {level}");
        }
    }

    [Fact]
    public void Validate_WithOptionalTraceIdAndMetadata_Succeeds()
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
            Location = "Test:42",
            TraceId = "trace-123",
            Metadata = "{\"key\":\"value\"}"
        };

        // Act
        var result = _validator.Validate(logEntry);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

/// <summary>
/// Comprehensive unit tests for LogQuery.
/// </summary>
public class LogQueryModelTests
{
    [Fact]
    public void LogQuery_DefaultValues_CreatesValidQuery()
    {
        // Arrange & Act
        var query = new LogQuery();

        // Assert
        query.Should().NotBeNull();
        query.PageSize.Should().Be(50);
        query.Ascending.Should().BeFalse();
        query.OrderBy.Should().Be("Timestamp");
    }

    [Fact]
    public void LogQuery_WithCustomPageSize_Succeeds()
    {
        // Arrange & Act
        var query = new LogQuery { PageSize = 50 };

        // Assert
        query.PageSize.Should().Be(50);
    }

    [Fact]
    public void LogQuery_Validate_WithValidQuery_ReturnsSuccess()
    {
        // Arrange
        var query = new LogQuery { PageSize = 10 };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void LogQuery_Validate_WithNegativePageSize_ReturnsFalse()
    {
        // Arrange
        var query = new LogQuery { PageSize = -1 };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("PageSize"));
    }

    [Fact]
    public void LogQuery_Validate_WithZeroPageSize_ReturnsFalse()
    {
        // Arrange
        var query = new LogQuery { PageSize = 0 };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void LogQuery_Validate_WithExcessivePageSize_ReturnsFalse()
    {
        // Arrange
        var query = new LogQuery { PageSize = 10001 };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("PageSize") || e.Contains("10000"));
    }

    [Fact]
    public void LogQuery_Validate_WithValidFilters_Succeeds()
    {
        // Arrange
        var query = new LogQuery
        {
            PageSize = 10,
            Filters = new Dictionary<string, object>
            {
                { "LogLevel", "Error" }
            }
        };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void LogQuery_Validate_WithMultipleFilters_Succeeds()
    {
        // Arrange
        var query = new LogQuery
        {
            PageSize = 10,
            Filters = new Dictionary<string, object>
            {
                { "LogLevel", "Error" },
                { "LoggerName", "TestApp" },
                { "TraceId", "trace-123" }
            }
        };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void LogQuery_Validate_WithInvalidOrderBy_FailsValidation()
    {
        // Arrange
        var query = new LogQuery
        {
            PageSize = 10,
            OrderBy = "InvalidField"
        };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeFalse(); // Invalid OrderBy field causes validation to fail
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void LogQuery_WithEmptyFilters_Succeeds()
    {
        // Arrange
        var query = new LogQuery
        {
            PageSize = 10,
            Filters = new Dictionary<string, object>()
        };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void LogQuery_OrderByDescending_Succeeds()
    {
        // Arrange
        var query = new LogQuery
        {
            PageSize = 10,
            OrderBy = "Timestamp",
            Ascending = true
        };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
        query.Ascending.Should().BeTrue();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void LogQuery_WithValidPageSizeRange_Succeeds(int pageSize)
    {
        // Arrange
        var query = new LogQuery { PageSize = pageSize };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Timestamp")]
    [InlineData("LogLevel")]
    [InlineData("LoggerName")]
    [InlineData("Location")]
    [InlineData("Message")]
    [InlineData("TraceId")]
    public void LogQuery_WithValidOrderByFields_Succeeds(string orderByField)
    {
        // Arrange
        var query = new LogQuery
        {
            PageSize = 10,
            OrderBy = orderByField
        };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
