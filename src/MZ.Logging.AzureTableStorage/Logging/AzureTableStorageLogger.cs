using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MZ.Logging.AzureTableStorage;

/// <summary>
/// Logger implementation for Azure Table Storage with async operations.
/// Provides methods for logging at various levels (Debug, Information, Warning,
/// Error, Critical). Supports metadata and exception logging with validation.
/// </summary>
public class AzureTableStorageLogger
{
    private readonly ILogStorage _storage;
    private readonly string _loggerName;
    private readonly string _defaultTraceId;
    private readonly ILogValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AzureTableStorageLogger"/>.
    /// </summary>
    /// <param name="storage">The log storage implementation.</param>
    /// <param name="loggerName">The name of the logger.</param>
    /// <param name="defaultTraceId">The default trace ID for log entries.</param>
    /// <param name="validator">The log validator (optional).</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when storage is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when loggerName or defaultTraceId is null, empty, or whitespace.
    /// </exception>
    public AzureTableStorageLogger(
        ILogStorage storage,
        string loggerName,
        string defaultTraceId,
        ILogValidator? validator = null)
    {
        ValidateInputs(storage, loggerName, defaultTraceId);
        _storage = storage;
        _loggerName = loggerName;
        _defaultTraceId = defaultTraceId;
        _validator = validator ?? new LogValidator();
    }

    /// <summary>
    /// Logs a debug message asynchronously.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="traceId">The trace ID (uses default if not provided).</param>
    /// <param name="exception">The exception associated with the log.</param>
    /// <param name="metadata">Additional metadata to include.</param>
    /// <param name="cancellationToken">
    /// The cancellation token for the operation.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DebugAsync(
        string message,
        string? traceId = null,
        Exception? exception = null,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(
            LogLevel.Debug,
            message,
            traceId,
            exception,
            metadata,
            cancellationToken);
    }

    /// <summary>
    /// Logs an information message asynchronously.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="traceId">The trace ID (uses default if not provided).</param>
    /// <param name="exception">The exception associated with the log.</param>
    /// <param name="metadata">Additional metadata to include.</param>
    /// <param name="cancellationToken">
    /// The cancellation token for the operation.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InformationAsync(
        string message,
        string? traceId = null,
        Exception? exception = null,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(
            LogLevel.Information,
            message,
            traceId,
            exception,
            metadata,
            cancellationToken);
    }

    /// <summary>
    /// Logs a warning message asynchronously.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="traceId">The trace ID (uses default if not provided).</param>
    /// <param name="exception">The exception associated with the log.</param>
    /// <param name="metadata">Additional metadata to include.</param>
    /// <param name="cancellationToken">
    /// The cancellation token for the operation.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task WarningAsync(
        string message,
        string? traceId = null,
        Exception? exception = null,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(
            LogLevel.Warning,
            message,
            traceId,
            exception,
            metadata,
            cancellationToken);
    }

    /// <summary>
    /// Logs an error message asynchronously.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="traceId">The trace ID (uses default if not provided).</param>
    /// <param name="exception">The exception associated with the log.</param>
    /// <param name="metadata">Additional metadata to include.</param>
    /// <param name="cancellationToken">
    /// The cancellation token for the operation.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ErrorAsync(
        string message,
        string? traceId = null,
        Exception? exception = null,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(
            LogLevel.Error,
            message,
            traceId,
            exception,
            metadata,
            cancellationToken);
    }

    /// <summary>
    /// Logs a critical message asynchronously.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="traceId">The trace ID (uses default if not provided).</param>
    /// <param name="exception">The exception associated with the log.</param>
    /// <param name="metadata">Additional metadata to include.</param>
    /// <param name="cancellationToken">
    /// The cancellation token for the operation.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CriticalAsync(
        string message,
        string? traceId = null,
        Exception? exception = null,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(
            LogLevel.Critical,
            message,
            traceId,
            exception,
            metadata,
            cancellationToken);
    }

    /// <summary>
    /// Retrieves logs asynchronously based on a query.
    /// </summary>
    /// <param name="query">The log query criteria.</param>
    /// <param name="cancellationToken">
    /// The cancellation token for the operation.
    /// </param>
    /// <returns>
    /// A tuple containing the list of log entries and continuation token
    /// for pagination.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when query is null.
    /// </exception>
    public async Task<(IReadOnlyList<LogEntry>, string?)> GetLogsAsync(
        LogQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        return await _storage.GetLogsAsync(query, cancellationToken);
    }

    /// <summary>
    /// Internal method that logs a message asynchronously with validation.
    /// </summary>
    private async Task LogAsync(
        LogLevel level,
        string message,
        string? traceId,
        Exception? exception,
        Dictionary<string, object>? metadata,
        CancellationToken cancellationToken)
    {
        // OWASP: Validate inputs to prevent injection attacks
        ValidateMessage(message);

        var timestamp = DateTimeOffset.UtcNow;
        var partitionKey = _loggerName;
        var rowKey = $"{timestamp:yyyyMMddHHmmssffffff}_{Guid.NewGuid():N}";

        var logEntry = new LogEntry
        {
            PartitionKey = partitionKey,
            RowKey = rowKey,
            LoggerName = _loggerName,
            Level = level,
            Message = message,
            TraceId = traceId ?? _defaultTraceId,
            Exception = exception?.ToString(),
            Metadata = SerializeMetadata(metadata),
            Timestamp = timestamp.ToString("o"),
            Location = string.Empty
        };

        // OWASP: Validate log entry before storing
        _validator.Validate(logEntry);

        await _storage.StoreLogAsync(partitionKey, rowKey, logEntry, cancellationToken);
    }

    private static void ValidateInputs(
        ILogStorage? storage,
        string? loggerName,
        string? defaultTraceId)
    {
        if (storage == null)
        {
            throw new ArgumentNullException(nameof(storage));
        }

        if (string.IsNullOrWhiteSpace(loggerName))
        {
            throw new ArgumentException(
                "Logger name cannot be null or empty.",
                nameof(loggerName));
        }

        if (string.IsNullOrWhiteSpace(defaultTraceId))
        {
            throw new ArgumentException(
                "Default trace ID cannot be null or empty.",
                nameof(defaultTraceId));
        }
    }

    private static void ValidateMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                "Message cannot be null or empty.",
                nameof(message));
        }
    }

    private static string? SerializeMetadata(
        Dictionary<string, object>? metadata)
    {
        if (metadata == null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Serialize(metadata);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException(
                "Metadata serialization failed.",
                nameof(metadata),
                ex);
        }
    }
}
