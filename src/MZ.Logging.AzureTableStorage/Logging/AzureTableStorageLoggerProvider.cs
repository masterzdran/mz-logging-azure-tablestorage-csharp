using Microsoft.Extensions.Logging;

namespace MZ.Logging.AzureTableStorage.Logging;

/// <summary>
/// Provides <see cref="ILogger"/> instances that log to Azure Table Storage.
/// Implements the factory pattern for creating logger instances compatible with
/// Microsoft.Extensions.Logging.
/// </summary>
public class AzureTableStorageLoggerProvider : ILoggerProvider
{
    private readonly ILogStorage _storage;
    private readonly string _loggerName;
    private readonly string _defaultTraceId;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="AzureTableStorageLoggerProvider"/>.
    /// </summary>
    /// <param name="storage">The Azure Table Storage instance.</param>
    /// <param name="loggerName">The default logger name.</param>
    /// <param name="defaultTraceId">The default trace ID.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when storage, loggerName, or defaultTraceId is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when loggerName or defaultTraceId is empty or whitespace.
    /// </exception>
    public AzureTableStorageLoggerProvider(
        ILogStorage storage,
        string loggerName,
        string defaultTraceId)
    {
        ValidateInputs(storage, loggerName, defaultTraceId);
        _storage = storage;
        _loggerName = loggerName;
        _defaultTraceId = defaultTraceId;
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(categoryName))
        {
            throw new ArgumentException(
                "Category name cannot be null or empty.",
                nameof(categoryName));
        }

        return new AzureTableStorageLoggerWrapper(
            _storage,
            categoryName,
            _defaultTraceId);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the provider and its resources.
    /// </summary>
    /// <param name="disposing">
    /// True to dispose managed resources; false for finalizer cleanup.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Dispose managed resources if any
        }

        _disposed = true;
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

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(
                nameof(AzureTableStorageLoggerProvider));
        }
    }
}