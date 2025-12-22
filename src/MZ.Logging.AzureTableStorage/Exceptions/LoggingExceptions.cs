namespace MZ.Logging.AzureTableStorage;

/// <summary>
/// Represents an exception that occurs during logging operations.
/// Implements OWASP secure exception handling practices.
/// </summary>
public class LoggingException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public LoggingException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LoggingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Represents an exception that occurs during storage operations.
/// </summary>
public class StorageException : LoggingException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StorageException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public StorageException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public StorageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Represents a validation exception that occurs during data validation.
/// </summary>
public class ValidationException : LoggingException
{
    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errors">The validation errors.</param>
    public ValidationException(string message, IReadOnlyList<string> errors) : base(message)
    {
        Errors = errors;
    }
}
