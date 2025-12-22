// filepath: src/MZ.Logging.AzureTableStorage/Logging/AzureTableStorageLoggerWrapper.cs
using Microsoft.Extensions.Logging;
using MZ.Logging.AzureTableStorage.Logging.MZ.Logging.AzureTableStorage;


namespace MZ.Logging.AzureTableStorage.Logging;

/// <summary>
/// Wraps <see cref="AzureTableStorageLogger"/> to implement <see cref="ILogger"/> 
/// for Microsoft.Extensions.Logging compatibility.
/// </summary>
/// <remarks>
/// This adapter bridges the synchronous ILogger interface with the async 
/// AzureTableStorageLogger. Async operations are scheduled via TaskScheduler 
/// to prevent blocking. For high-throughput scenarios, consider using the 
/// AzureTableStorageLogger directly with async methods.
/// </remarks>
public class AzureTableStorageLoggerWrapper : ILogger
{
    private static class MetadataKeys
    {
        public const string EventId = "EventId";
        public const string EventName = "EventName";
    }

    private readonly AzureTableStorageLogger _logger;
    private readonly TaskScheduler _taskScheduler;

    /// <summary>
    /// Initializes a new instance of <see cref="AzureTableStorageLoggerWrapper"/>.
    /// </summary>
    /// <param name="storage">The log storage implementation.</param>
    /// <param name="categoryName">The logger category name.</param>
    /// <param name="defaultTraceId">The default trace ID.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when storage is null or defaultTraceId is null/empty.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when categoryName or defaultTraceId is null/empty.
    /// </exception>
    public AzureTableStorageLoggerWrapper(
        ILogStorage storage,
        string categoryName,
        string defaultTraceId)
        : this(storage, categoryName, defaultTraceId, TaskScheduler.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="AzureTableStorageLoggerWrapper"/> 
    /// with a custom task scheduler (for testing).
    /// </summary>
    /// <param name="storage">The log storage implementation.</param>
    /// <param name="categoryName">The logger category name.</param>
    /// <param name="defaultTraceId">The default trace ID.</param>
    /// <param name="taskScheduler">The task scheduler for async operations.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any parameter is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when categoryName or defaultTraceId is null/empty.
    /// </exception>
    internal AzureTableStorageLoggerWrapper(
        ILogStorage storage,
        string categoryName,
        string defaultTraceId,
        TaskScheduler taskScheduler)
    {
        ValidateInputs(storage, categoryName, defaultTraceId, taskScheduler);

        _logger = new AzureTableStorageLogger(storage, categoryName, defaultTraceId);
        _taskScheduler = taskScheduler;
    }

    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        // Scopes are not supported by this implementation; return null per ILogger contract
        return null;
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        // Always enabled; filtering handled by upstream logging framework
        return true;
    }

    /// <inheritdoc />
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!this.IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);
        var metadata = BuildMetadata(eventId);

        // Schedule async work to avoid blocking the caller
        var task = Task.Factory.StartNew(
            () => LogAsync(logLevel, message, exception, metadata),
            CancellationToken.None,
            TaskCreationOptions.DenyChildAttach,
            _taskScheduler);

        // Do not await to maintain synchronous contract; observe exceptions via continuations
        _ = task.ContinueWith(
            LogTaskFaulted,
            TaskScheduler.Default);
    }

    private static void ValidateInputs(
        ILogStorage? storage,
        string? categoryName,
        string? defaultTraceId,
        TaskScheduler? taskScheduler)
    {
        if (storage == null)
        {
            throw new ArgumentNullException(nameof(storage));
        }

        if (string.IsNullOrWhiteSpace(categoryName))
        {
            throw new ArgumentException(
                "Category name cannot be null or empty.",
                nameof(categoryName));
        }

        if (string.IsNullOrWhiteSpace(defaultTraceId))
        {
            throw new ArgumentException(
                "Default trace ID cannot be null or empty.",
                nameof(defaultTraceId));
        }

        if (taskScheduler == null)
        {
            throw new ArgumentNullException(nameof(taskScheduler));
        }
    }

    private static Dictionary<string, object> BuildMetadata(EventId eventId)
    {
        return new Dictionary<string, object>
        {
            { MetadataKeys.EventId, eventId.Id },
            { MetadataKeys.EventName, eventId.Name ?? string.Empty }
        };
    }

    private async Task LogAsync(
        LogLevel logLevel,
        string message,
        Exception? exception,
        Dictionary<string, object> metadata)
    {
        try
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    await _logger.DebugAsync(
                        message,
                        exception: exception,
                        metadata: metadata);
                    break;

                case LogLevel.Information:
                    await _logger.InformationAsync(
                        message,
                        exception: exception,
                        metadata: metadata);
                    break;

                case LogLevel.Warning:
                    await _logger.WarningAsync(
                        message,
                        exception: exception,
                        metadata: metadata);
                    break;

                case LogLevel.Error:
                    await _logger.ErrorAsync(
                        message,
                        exception: exception,
                        metadata: metadata);
                    break;

                case LogLevel.Critical:
                    await _logger.CriticalAsync(
                        message,
                        exception: exception,
                        metadata: metadata);
                    break;

                default:
                    await _logger.InformationAsync(
                        $"Unknown log level: {logLevel} - {message}",
                        exception: exception,
                        metadata: metadata);
                    break;
            }
        }
        catch (Exception ex)
        {
            // OWASP: Sanitize error info; avoid exposing implementation details
            LogFallback($"Async logging failed: {ex.GetType().Name}");
        }
    }

    private static void LogTaskFaulted(Task task)
    {
        if (task.IsFaulted)
        {
            LogFallback(
                $"Logging task exception: {task.Exception?.InnerException?.GetType().Name}");
        }
    }

    private static void LogFallback(string message)
    {
        // Fallback for logging failures; consider configuration for production
        try
        {
            Console.Error.WriteLine($"[{DateTime.UtcNow:O}] {message}");
        }
        catch
        {
            // Suppress all exceptions to prevent cascading failures
        }
    }

}