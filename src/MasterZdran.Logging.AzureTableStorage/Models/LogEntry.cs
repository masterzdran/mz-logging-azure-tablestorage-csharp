using Microsoft.Extensions.Logging;

namespace MasterZdran.Logging.AzureTableStorage;


/// <summary>
/// Represents a complete log entry with metadata.
/// </summary>
public class LogEntry
{
    /// <summary>
    /// Gets or sets the partition key.
    /// </summary>
    public required string PartitionKey { get; set; }

    /// <summary>
    /// Gets or sets the row key.
    /// </summary>
    public required string RowKey { get; set; }

    /// <summary>
    /// Gets or sets the log level.
    /// </summary>
    public LogLevel Level { get; set; }

    /// <summary>
    /// Gets or sets the log message.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Gets or sets the timestamp in ISO 8601 format.
    /// </summary>
    public required string Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the trace ID for distributed tracing.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Gets or sets the logger name.
    /// </summary>
    public required string LoggerName { get; set; }

    /// <summary>
    /// Gets or sets the caller location (module:line).
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets custom metadata as a JSON string.
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the exception information if any.
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// Validates the log entry.
    /// </summary>
    /// <returns>Validation result with errors if any.</returns>
    public ValidationResult Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(PartitionKey))
            errors.Add("PartitionKey cannot be empty.");

        if (string.IsNullOrWhiteSpace(RowKey))
            errors.Add("RowKey cannot be empty.");

        if (string.IsNullOrWhiteSpace(Message))
            errors.Add("Message cannot be empty.");

        if (string.IsNullOrWhiteSpace(LoggerName))
            errors.Add("LoggerName cannot be empty.");

        if (string.IsNullOrWhiteSpace(Timestamp))
            errors.Add("Timestamp cannot be empty.");

        // Validate timestamp format
        if (!DateTime.TryParse(Timestamp, out _))
            errors.Add("Timestamp must be in valid ISO 8601 format.");

        return new ValidationResult { IsValid = !errors.Any(), Errors = errors };
    }
}
