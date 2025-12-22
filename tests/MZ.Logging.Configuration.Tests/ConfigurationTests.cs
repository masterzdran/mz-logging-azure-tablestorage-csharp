using Microsoft.Extensions.Configuration;
using Xunit;
using FluentAssertions;

namespace MZ.Logging.Configuration.Tests;

/// <summary>
/// Comprehensive unit tests for AzureTableStorageLoggingConfiguration.
/// </summary>
public class AzureTableStorageLoggingConfigurationTests
{
    [Fact]
    public void Configuration_WithAllValidFields_PassesValidation()
    {
        // Arrange
        var config = new AzureTableStorageLoggingConfiguration
        {
            ConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;",
            TableName = "Logs",
            LoggerName = "TestLogger",
            DefaultTraceId = "trace-123",
            EnvironmentName = "Production"
        };

        // Act
        var validationErrors = config.Validate();

        // Assert
        validationErrors.Should().BeEmpty();
    }

    [Fact]
    public void Configuration_WithMissingConnectionString_FailsValidation()
    {
        // Arrange
        var config = new AzureTableStorageLoggingConfiguration
        {
            TableName = "Logs",
            LoggerName = "TestLogger"
        };

        // Act
        var validationErrors = config.Validate();

        // Assert
        validationErrors.Should().Contain(e => e.Contains("ConnectionString"));
    }

    [Fact]
    public void Configuration_WithEmptyConnectionString_FailsValidation()
    {
        // Arrange
        var config = new AzureTableStorageLoggingConfiguration
        {
            ConnectionString = "",
            TableName = "Logs",
            LoggerName = "TestLogger"
        };

        // Act
        var validationErrors = config.Validate();

        // Assert
        validationErrors.Should().Contain(e => e.Contains("ConnectionString"));
    }

    [Fact]
    public void Configuration_WithMissingTableName_FailsValidation()
    {
        // Arrange
        var config = new AzureTableStorageLoggingConfiguration
        {
            ConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;",
            LoggerName = "TestLogger"
        };

        // Act
        var validationErrors = config.Validate();

        // Assert
        validationErrors.Should().Contain(e => e.Contains("TableName"));
    }

    [Fact]
    public void Configuration_WithEmptyTableName_FailsValidation()
    {
        // Arrange
        var config = new AzureTableStorageLoggingConfiguration
        {
            ConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;",
            TableName = "",
            LoggerName = "TestLogger"
        };

        // Act
        var validationErrors = config.Validate();

        // Assert
        validationErrors.Should().Contain(e => e.Contains("TableName"));
    }

    [Fact]
    public void Configuration_WithMissingLoggerName_FailsValidation()
    {
        // Arrange
        var config = new AzureTableStorageLoggingConfiguration
        {
            ConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;",
            TableName = "Logs"
        };

        // Act
        var validationErrors = config.Validate();

        // Assert
        validationErrors.Should().Contain(e => e.Contains("LoggerName"));
    }

    [Fact]
    public void Configuration_WithEmptyLoggerName_FailsValidation()
    {
        // Arrange
        var config = new AzureTableStorageLoggingConfiguration
        {
            ConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;",
            TableName = "Logs",
            LoggerName = ""
        };

        // Act
        var validationErrors = config.Validate();

        // Assert
        validationErrors.Should().Contain(e => e.Contains("LoggerName"));
    }

    [Fact]
    public void Configuration_WithAllMissingFields_FailsValidation()
    {
        // Arrange
        var config = new AzureTableStorageLoggingConfiguration();

        // Act
        var validationErrors = config.Validate();

        // Assert
        validationErrors.Should().HaveCount(3);
        validationErrors.Should().Contain(e => e.Contains("ConnectionString"));
        validationErrors.Should().Contain(e => e.Contains("TableName"));
        validationErrors.Should().Contain(e => e.Contains("LoggerName"));
    }

    [Fact]
    public void Configuration_WithOptionalFieldsMissing_StillValid()
    {
        // Arrange
        var config = new AzureTableStorageLoggingConfiguration
        {
            ConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;",
            TableName = "Logs",
            LoggerName = "TestLogger"
            // DefaultTraceId and EnvironmentName are optional
        };

        // Act
        var validationErrors = config.Validate();

        // Assert
        validationErrors.Should().BeEmpty();
    }

    [Fact]
    public void Configuration_Properties_CanBeSet()
    {
        // Arrange
        var config = new AzureTableStorageLoggingConfiguration();

        // Act
        config.ConnectionString = "connection";
        config.TableName = "table";
        config.LoggerName = "logger";
        config.DefaultTraceId = "trace";
        config.EnvironmentName = "dev";

        // Assert
        config.ConnectionString.Should().Be("connection");
        config.TableName.Should().Be("table");
        config.LoggerName.Should().Be("logger");
        config.DefaultTraceId.Should().Be("trace");
        config.EnvironmentName.Should().Be("dev");
    }

    [Fact]
    public void Configuration_WithWhitespaceConnectionString_FailsValidation()
    {
        // Arrange
        var config = new AzureTableStorageLoggingConfiguration
        {
            ConnectionString = "   ",
            TableName = "Logs",
            LoggerName = "TestLogger"
        };

        // Act
        var validationErrors = config.Validate();

        // Assert
        validationErrors.Should().Contain(e => e.Contains("ConnectionString"));
    }

    [Fact]
    public void Configuration_WithWhitespaceTableName_FailsValidation()
    {
        // Arrange
        var config = new AzureTableStorageLoggingConfiguration
        {
            ConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;",
            TableName = "   ",
            LoggerName = "TestLogger"
        };

        // Act
        var validationErrors = config.Validate();

        // Assert
        validationErrors.Should().Contain(e => e.Contains("TableName"));
    }

    [Fact]
    public void Configuration_WithWhitespaceLoggerName_FailsValidation()
    {
        // Arrange
        var config = new AzureTableStorageLoggingConfiguration
        {
            ConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;",
            TableName = "Logs",
            LoggerName = "   "
        };

        // Act
        var validationErrors = config.Validate();

        // Assert
        validationErrors.Should().Contain(e => e.Contains("LoggerName"));
    }
}

/// <summary>
/// Comprehensive unit tests for ConfigurationBuilderExtensions.
/// </summary>
public class ConfigurationBuilderExtensionsTests
{
    [Fact]
    public void GetAzureTableStorageLoggingConfiguration_WithValidConfig_ReturnsConfiguration()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        var config = new Dictionary<string, string>
        {
            { "AzureTableStorageLogging:ConnectionString", "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;" },
            { "AzureTableStorageLogging:TableName", "Logs" },
            { "AzureTableStorageLogging:LoggerName", "TestLogger" }
        };
        configBuilder.AddInMemoryCollection(config);

        var configuration = configBuilder.Build();

        // Act
        var loggingConfig = configuration.GetAzureTableStorageLoggingConfiguration();

        // Assert
        loggingConfig.Should().NotBeNull();
        loggingConfig.ConnectionString.Should().Be("DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;");
        loggingConfig.TableName.Should().Be("Logs");
        loggingConfig.LoggerName.Should().Be("TestLogger");
    }

    [Fact]
    public void GetAzureTableStorageLoggingConfiguration_WithMissingRequiredField_ThrowsInvalidOperationException()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        var config = new Dictionary<string, string>
        {
            { "AzureTableStorageLogging:ConnectionString", "connection" },
            { "AzureTableStorageLogging:TableName", "Logs" }
            // Missing LoggerName
        };
        configBuilder.AddInMemoryCollection(config);
        var configuration = configBuilder.Build();

        // Act & Assert
        var action = () => configuration.GetAzureTableStorageLoggingConfiguration();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Invalid logging configuration*");
    }

    [Fact]
    public void GetAzureTableStorageLoggingConfiguration_WithCustomSectionName_ReturnsConfiguration()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        var config = new Dictionary<string, string>
        {
            { "CustomLogging:ConnectionString", "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;" },
            { "CustomLogging:TableName", "Logs" },
            { "CustomLogging:LoggerName", "TestLogger" }
        };
        configBuilder.AddInMemoryCollection(config);
        var configuration = configBuilder.Build();

        // Act
        var loggingConfig = configuration.GetAzureTableStorageLoggingConfiguration("CustomLogging");

        // Assert
        loggingConfig.Should().NotBeNull();
        loggingConfig.LoggerName.Should().Be("TestLogger");
    }

    [Fact]
    public void GetAzureTableStorageLoggingConfiguration_WithOptionalFields_ReturnsConfiguration()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        var config = new Dictionary<string, string>
        {
            { "AzureTableStorageLogging:ConnectionString", "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;" },
            { "AzureTableStorageLogging:TableName", "Logs" },
            { "AzureTableStorageLogging:LoggerName", "TestLogger" },
            { "AzureTableStorageLogging:DefaultTraceId", "trace-123" },
            { "AzureTableStorageLogging:EnvironmentName", "Production" }
        };
        configBuilder.AddInMemoryCollection(config);
        var configuration = configBuilder.Build();

        // Act
        var loggingConfig = configuration.GetAzureTableStorageLoggingConfiguration();

        // Assert
        loggingConfig.DefaultTraceId.Should().Be("trace-123");
        loggingConfig.EnvironmentName.Should().Be("Production");
    }

    [Fact]
    public void AddAzureKeyVault_WithNullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        IConfigurationBuilder nullBuilder = null!;

        // Act & Assert
        var action = () => nullBuilder.AddAzureKeyVault("https://keyvault.azure.com");
        action.Should().Throw<ArgumentNullException>().WithParameterName("configurationBuilder");
    }

    [Fact]
    public void AddAzureKeyVault_WithEmptyUri_ThrowsArgumentException()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();

        // Act & Assert
        var action = () => configBuilder.AddAzureKeyVault("");
        action.Should().Throw<ArgumentException>().WithParameterName("keyVaultUri");
    }

    [Fact]
    public void AddAzureKeyVault_WithValidUri_ReturnsBuilder()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();

        // Act
        var result = configBuilder.AddAzureKeyVault("https://keyvault.azure.com");

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(configBuilder); // Should return builder for chaining
    }

    [Fact]
    public void AddAzureAppConfiguration_WithNullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        IConfigurationBuilder nullBuilder = null!;

        // Act & Assert
        var action = () => nullBuilder.AddAzureAppConfiguration("connection");
        action.Should().Throw<ArgumentNullException>().WithParameterName("configurationBuilder");
    }

    [Fact]
    public void AddAzureAppConfiguration_WithEmptyConnectionString_ThrowsArgumentException()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();

        // Act & Assert
        var action = () => configBuilder.AddAzureAppConfiguration("");
        action.Should().Throw<ArgumentException>().WithParameterName("connectionString");
    }

    [Fact]
    public void AddAzureAppConfiguration_WithValidConnectionString_ReturnsBuilder()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();

        // Act
        var result = configBuilder.AddAzureAppConfiguration("Endpoint=https://test.azconfig.io;Id=test;Secret=test");

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(configBuilder); // Should return builder for chaining
    }

    [Fact]
    public void ConfigurationBuilder_FluentChaining_AllMethodsWork()
    {
        // Arrange
        var config = new Dictionary<string, string>
        {
            { "AzureTableStorageLogging:ConnectionString", "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;" },
            { "AzureTableStorageLogging:TableName", "Logs" },
            { "AzureTableStorageLogging:LoggerName", "TestLogger" }
        };

        // Act
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(config);

        // Act & Assert - should not throw
        var loggingConfig = configBuilder.Build()
            .GetAzureTableStorageLoggingConfiguration();

        loggingConfig.Should().NotBeNull();
    }

    [Fact]
    public void GetAzureTableStorageLoggingConfiguration_WithAllEmptyFields_ThrowsInvalidOperationException()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        var config = new Dictionary<string, string>
        {
            { "AzureTableStorageLogging:ConnectionString", "" },
            { "AzureTableStorageLogging:TableName", "" },
            { "AzureTableStorageLogging:LoggerName", "" }
        };
        configBuilder.AddInMemoryCollection(config);
        var configuration = configBuilder.Build();

        // Act & Assert
        var action = () => configuration.GetAzureTableStorageLoggingConfiguration();
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetAzureTableStorageLoggingConfiguration_WithWhitespaceFields_ThrowsInvalidOperationException()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        var config = new Dictionary<string, string>
        {
            { "AzureTableStorageLogging:ConnectionString", "   " },
            { "AzureTableStorageLogging:TableName", "   " },
            { "AzureTableStorageLogging:LoggerName", "   " }
        };
        configBuilder.AddInMemoryCollection(config);
        var configuration = configBuilder.Build();

        // Act & Assert
        var action = () => configuration.GetAzureTableStorageLoggingConfiguration();
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddAzureKeyVault_WithWhitespaceUri_ThrowsArgumentException()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();

        // Act & Assert
        var action = () => configBuilder.AddAzureKeyVault("   ");
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddAzureAppConfiguration_WithWhitespaceConnectionString_ThrowsArgumentException()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();

        // Act & Assert
        var action = () => configBuilder.AddAzureAppConfiguration("   ");
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetAzureTableStorageLoggingConfiguration_NonExistentSection_ThrowsInvalidOperationException()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string>());
        var configuration = configBuilder.Build();

        // Act & Assert
        var action = () => configuration.GetAzureTableStorageLoggingConfiguration();
        action.Should().Throw<InvalidOperationException>();
    }
}
