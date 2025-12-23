namespace MasterZdran.Logging.Configuration;

/// <summary>
/// Configuration settings for Azure Table Storage logging.
/// </summary>
public class AzureTableStorageLoggingConfiguration
{
    /// <summary>
    /// Gets or sets the Azure Storage connection string.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the table name for storing logs.
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// Gets or sets the logger name.
    /// </summary>
    public string? LoggerName { get; set; }

    /// <summary>
    /// Gets or sets the default trace ID for correlation.
    /// </summary>
    public string? DefaultTraceId { get; set; }

    /// <summary>
    /// Gets or sets the environment name (Development, Staging, Production).
    /// </summary>
    public string? EnvironmentName { get; set; }

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    /// <returns>Validation errors if any.</returns>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ConnectionString))
            errors.Add("ConnectionString is required.");

        if (string.IsNullOrWhiteSpace(TableName))
            errors.Add("TableName is required.");

        if (string.IsNullOrWhiteSpace(LoggerName))
            errors.Add("LoggerName is required.");

        return errors;
    }
}
