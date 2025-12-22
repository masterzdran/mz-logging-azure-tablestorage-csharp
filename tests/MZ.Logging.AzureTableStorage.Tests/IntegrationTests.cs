using Xunit;
using FluentAssertions;

namespace MZ.Logging.AzureTableStorage.Tests;

/// <summary>
/// Additional focused integration-style tests for core library functionality.
/// </summary>
public class IntegrationTests
{
    [Fact]
    public void LogEntry_CanBeValidated()
    {
        // Arrange
        var entry = new LogEntry
        {
            PartitionKey = "App1",
            RowKey = "2024-01-01T12:00:00",
            Message = "Test log",
            LoggerName = "TestLogger",
            Location = "TestClass.cs:42",
            Timestamp = "2024-01-01T12:00:00Z",
            Level = LogLevel.Information
        };

        // Act
        var result = entry.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void LogQuery_CanBeValidated()
    {
        // Arrange
        var query = new LogQuery 
        { 
            PageSize = 25, 
            OrderBy = "LogLevel" 
        };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Timestamp")]
    [InlineData("LogLevel")]
    [InlineData("TraceId")]
    [InlineData("LoggerName")]
    [InlineData("Location")]
    [InlineData("Message")]
    public void LogQuery_WithValidOrderByFields_Validates(string orderBy)
    {
        // Arrange
        var query = new LogQuery { PageSize = 50, OrderBy = orderBy };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(500)]
    [InlineData(1000)]
    public void LogQuery_WithValidPageSizes_Validates(int pageSize)
    {
        // Arrange
        var query = new LogQuery { PageSize = pageSize };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void LogQuery_WithInvalidPageSizes_FailsValidation(int pageSize)
    {
        // Arrange
        var query = new LogQuery { PageSize = pageSize };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void LogQuery_With1001PageSize_FailsValidation()
    {
        // Arrange
        var query = new LogQuery { PageSize = 1001 };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("1000"));
    }

    [Fact]
    public void LogQuery_WithEmptyOrderBy_FailsValidation()
    {
        // Arrange
        var query = new LogQuery { PageSize = 50, OrderBy = "" };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("OrderBy"));
    }

    [Fact]
    public void LogQuery_WithInvalidOrderByField_FailsValidation()
    {
        // Arrange
        var query = new LogQuery { PageSize = 50, OrderBy = "InvalidField" };

        // Act
        var result = query.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("OrderBy"));
    }

    [Fact]
    public void LogQuery_WithFilters_CanBeCreated()
    {
        // Arrange & Act
        var query = new LogQuery 
        { 
            PageSize = 50,
            Filters = new Dictionary<string, object>
            {
                { "LogLevel", "Error" },
                { "LoggerName", "MyApp" }
            }
        };

        // Assert
        query.Filters.Should().HaveCount(2);
        query.Filters.Should().ContainKey("LogLevel");
        query.Filters.Should().ContainKey("LoggerName");
    }

    [Fact]
    public void LogLevel_EnumHasAllExpectedValues()
    {
        // Arrange & Act & Assert
        Assert.True(Enum.IsDefined(typeof(LogLevel), LogLevel.Debug));
        Assert.True(Enum.IsDefined(typeof(LogLevel), LogLevel.Information));
        Assert.True(Enum.IsDefined(typeof(LogLevel), LogLevel.Warning));
        Assert.True(Enum.IsDefined(typeof(LogLevel), LogLevel.Error));
        Assert.True(Enum.IsDefined(typeof(LogLevel), LogLevel.Critical));
    }

    [Fact]
    public void LogLevel_Values_AreInCorrectOrder()
    {
        // Assert
        ((int)LogLevel.Debug).Should().Be(0);
        ((int)LogLevel.Information).Should().Be(1);
        ((int)LogLevel.Warning).Should().Be(2);
        ((int)LogLevel.Error).Should().Be(3);
        ((int)LogLevel.Critical).Should().Be(4);
    }

    [Fact]
    public void ValidationResult_WithErrors_StoresErrorMessages()
    {
        // Arrange
        var result = new ValidationResult 
        { 
            IsValid = false,
            Errors = new List<string> 
            { 
                "Error 1", 
                "Error 2" 
            }
        };

        // Act & Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors[0].Should().Be("Error 1");
        result.Errors[1].Should().Be("Error 2");
    }

    [Fact]
    public void ValidationResult_WithoutErrors_IsValid()
    {
        // Arrange
        var result = new ValidationResult 
        { 
            IsValid = true,
            Errors = new List<string>()
        };

        // Act & Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void LogEntry_WithoutOptionalFields_IsValid()
    {
        // Arrange
        var entry = new LogEntry
        {
            PartitionKey = "App",
            RowKey = "row",
            Message = "msg",
            LoggerName = "logger",
            Location = "loc:1",
            Timestamp = DateTime.UtcNow.ToString("O")
        };

        // Act
        var result = entry.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
        entry.TraceId.Should().BeNull();
        entry.Metadata.Should().BeNull();
    }

    [Fact]
    public void LogEntry_WithAllOptionalFields_IsValid()
    {
        // Arrange
        var entry = new LogEntry
        {
            PartitionKey = "App",
            RowKey = "row",
            Message = "msg",
            LoggerName = "logger",
            Location = "loc:1",
            Timestamp = DateTime.UtcNow.ToString("O"),
            Level = LogLevel.Error,
            TraceId = "trace-123",
            Metadata = "{\"key\":\"value\"}"
        };

        // Act
        var result = entry.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
        entry.TraceId.Should().Be("trace-123");
        entry.Metadata.Should().Contain("key");
    }

    [Fact]
    public void LogValidator_ValidatesLogEntryCorrectly()
    {
        // Arrange
        var validator = new LogValidator();
        var validEntry = new LogEntry
        {
            PartitionKey = "App",
            RowKey = "row",
            Message = "msg",
            LoggerName = "logger",
            Location = "loc:1",
            Timestamp = DateTime.UtcNow.ToString("O")
        };

        // Act
        var result = validator.Validate(validEntry);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void LogValidator_RejectsInvalidEntry()
    {
        // Arrange
        var validator = new LogValidator();
        var invalidEntry = new LogEntry
        {
            PartitionKey = "",
            RowKey = "",
            Message = "",
            LoggerName = "",
            Location = "",
            Timestamp = "not-a-date"
        };

        // Act
        var result = validator.Validate(invalidEntry);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Count.Should().BeGreaterThan(5);
    }

    [Theory]
    [InlineData("2024-01-01T00:00:00Z")]
    [InlineData("2024-12-31T23:59:59Z")]
    [InlineData("2024-01-01T12:30:45.123Z")]
    public void LogEntry_WithValidIso8601Timestamps_Validates(string timestamp)
    {
        // Arrange
        var entry = new LogEntry
        {
            PartitionKey = "App",
            RowKey = "row",
            Message = "msg",
            LoggerName = "logger",
            Location = "loc:1",
            Timestamp = timestamp
        };

        // Act
        var result = entry.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("2024-13-01")]
    [InlineData("not a timestamp")]
    [InlineData("")]
    public void LogEntry_WithInvalidTimestamps_FailsValidation(string timestamp)
    {
        // Arrange
        var entry = new LogEntry
        {
            PartitionKey = "App",
            RowKey = "row",
            Message = "msg",
            LoggerName = "logger",
            Location = "loc:1",
            Timestamp = timestamp
        };

        // Act
        var result = entry.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Timestamp"));
    }

    [Fact]
    public void LogQuery_DefaultContinuationTokenIsNull()
    {
        // Arrange & Act
        var query = new LogQuery();

        // Assert
        query.ContinuationToken.Should().BeNull();
    }

    [Fact]
    public void LogQuery_ContinuationTokenCanBeSet()
    {
        // Arrange
        var query = new LogQuery { ContinuationToken = "token-123" };

        // Act & Assert
        query.ContinuationToken.Should().Be("token-123");
    }

    [Fact]
    public void LogQuery_WithNullFilters_InitializesAsEmpty()
    {
        // Arrange & Act
        var query = new LogQuery();

        // Assert
        query.Filters.Should().NotBeNull();
        query.Filters.Should().BeEmpty();
    }

    [Fact]
    public void LogEntry_LongPartitionKey_RemainsValid()
    {
        // Arrange
        var longKey = new string('a', 1024);
        var entry = new LogEntry
        {
            PartitionKey = longKey,
            RowKey = "row",
            Message = "msg",
            LoggerName = "logger",
            Location = "loc:1",
            Timestamp = DateTime.UtcNow.ToString("O")
        };

        // Act
        var result = entry.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void LogEntry_LongMessage_RemainsValid()
    {
        // Arrange
        var longMessage = new string('x', 4000);
        var entry = new LogEntry
        {
            PartitionKey = "App",
            RowKey = "row",
            Message = longMessage,
            LoggerName = "logger",
            Location = "loc:1",
            Timestamp = DateTime.UtcNow.ToString("O")
        };

        // Act
        var result = entry.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
