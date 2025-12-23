namespace MasterZdran.Logging.AzureTableStorage;

/// <summary>
/// Defines the contract for log storage implementations.
/// </summary>
public interface ILogStorage
{
    /// <summary>
    /// Stores a log entry asynchronously.
    /// </summary>
    /// <param name="partitionKey">The partition key for the log entry.</param>
    /// <param name="rowKey">The row key for the log entry.</param>
    /// <param name="logEntry">The log entry data to store.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StoreLogAsync(string partitionKey, string rowKey, LogEntry logEntry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves logs asynchronously with pagination and filtering.
    /// </summary>
    /// <param name="query">The log query parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing the list of logs and continuation token.</returns>
    Task<(IReadOnlyList<LogEntry> Logs, string? ContinuationToken)> GetLogsAsync(LogQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single log entry asynchronously.
    /// </summary>
    /// <param name="partitionKey">The partition key of the log entry.</param>
    /// <param name="rowKey">The row key of the log entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The log entry if found; otherwise null.</returns>
    Task<LogEntry?> GetLogEntryAsync(string partitionKey, string rowKey, CancellationToken cancellationToken = default);
}
