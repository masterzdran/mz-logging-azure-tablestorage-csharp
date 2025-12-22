namespace MZ.Logging.AzureTableStorage;

/// <summary>
/// Defines the contract for log validation.
/// </summary>
public interface ILogValidator
{
    /// <summary>
    /// Validates a log entry.
    /// </summary>
    /// <param name="logEntry">The log entry to validate.</param>
    /// <returns>Validation result.</returns>
    ValidationResult Validate(LogEntry logEntry);
}

/// <summary>
/// Default implementation of <see cref="ILogValidator"/>.
/// </summary>
public class LogValidator : ILogValidator
{
    /// <summary>
    /// Validates a log entry.
    /// </summary>
    /// <param name="logEntry">The log entry to validate.</param>
    /// <returns>Validation result with any errors.</returns>
    public ValidationResult Validate(LogEntry logEntry)
    {
        ArgumentNullException.ThrowIfNull(logEntry);
        return logEntry.Validate();
    }
}
