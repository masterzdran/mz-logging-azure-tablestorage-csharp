using Azure.Data.Tables;
using Microsoft.Extensions.Logging;

namespace MZ.Logging.AzureTableStorage.Storage;

/// <summary>
/// Azure Table Storage implementation of <see cref="ILogStorage"/>.
/// Provides secure, validated storage operations with OWASP best practices.
/// Handles log persistence, retrieval, and filtering with OData injection prevention.
/// </summary>
public class AzureTableStorage : ILogStorage
{
    private static class Constants
    {
        public const int MaxMessageLength = 4000;
        public const int AzuritePort = 10002;
        public const int HttpConflictStatusCode = 409;
        public const int HttpNotFoundStatusCode = 404;
        public const string AzuriteConnectionString = "UseDevelopmentStorage=true";
        public const string AzuriteUri = "http://127.0.0.1:10002/devstoreaccount1";
        public const string AzuriteAccountName = "devstoreaccount1";
        public const string AzuriteAccountKey =
            "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";
        public const string AzuriteFullConnectionString =
            "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=" +
            "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;" +
            "TableEndpoint=http://127.0.0.1:10002/devstoreaccount1";
    }

    private static class EntityPropertyNames
    {
        public const string LogLevel = "LogLevel";
        public const string Message = "Message";
        public const string Timestamp = "Timestamp";
        public const string TraceId = "TraceId";
        public const string LoggerName = "LoggerName";
        public const string Location = "Location";
        public const string Metadata = "Metadata";
        public const string Exception = "Exception";
    }

    private readonly TableClient _tableClient;
    private readonly ILogValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureTableStorage"/> class.
    /// </summary>
    /// <param name="connectionString">
    /// The Azure Storage connection string. Supports standard connection strings
    /// and Azurite development storage.
    /// </param>
    /// <param name="tableName">The name of the table to store logs.</param>
    /// <param name="validator">
    /// The log validator instance. Uses default if not provided.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when connection string or table name is invalid.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when connectionString or tableName is null.
    /// </exception>
    public AzureTableStorage(
        string connectionString,
        string tableName,
        ILogValidator? validator = null)
    {
        ValidateConnectionString(connectionString);
        ValidateTableName(tableName);

        var expandedConnectionString = ExpandAzuriteConnectionString(connectionString);
        var tableServiceClient = new TableServiceClient(expandedConnectionString);
        _tableClient = tableServiceClient.GetTableClient(tableName);
        _validator = validator ?? new LogValidator();

        CreateTableIfNotExistsAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureTableStorage"/> class
    /// with an explicit table client (useful for testing).
    /// </summary>
    /// <param name="tableClient">The Azure Table client.</param>
    /// <param name="validator">
    /// The log validator instance. Uses default if not provided.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when tableClient is null.
    /// </exception>
    public AzureTableStorage(
        TableClient tableClient,
        ILogValidator? validator = null)
    {
        _tableClient = tableClient ?? throw new ArgumentNullException(nameof(tableClient));
        _validator = validator ?? new LogValidator();
    }

    /// <summary>
    /// Stores a log entry in Azure Table Storage.
    /// </summary>
    /// <param name="partitionKey">The partition key for the log entry.</param>
    /// <param name="rowKey">The row key for the log entry.</param>
    /// <param name="logEntry">The log entry to store.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when logEntry is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when partition key or row key is invalid.
    /// </exception>
    /// <exception cref="ValidationException">
    /// Thrown when log entry validation fails.
    /// </exception>
    /// <exception cref="StorageException">
    /// Thrown when storage operation fails.
    /// </exception>
    public async Task StoreLogAsync(
        string partitionKey,
        string rowKey,
        LogEntry logEntry,
        CancellationToken cancellationToken = default)
    {
        ValidatePartitionAndRowKeys(partitionKey, rowKey);

        if (logEntry == null)
        {
            throw new ArgumentNullException(nameof(logEntry));
        }

        var validationResult = logEntry.Validate();
        if (!validationResult.IsValid)
        {
            throw new ValidationException(
                "Invalid log entry.",
                validationResult.Errors);
        }

        var entity = new TableEntity(partitionKey, rowKey)
        {
            { EntityPropertyNames.LogLevel, logEntry.Level.ToString() },
            { EntityPropertyNames.Message, SanitizeInput(logEntry.Message) },
            { EntityPropertyNames.Timestamp, logEntry.Timestamp },
            { EntityPropertyNames.TraceId, logEntry.TraceId ?? string.Empty },
            { EntityPropertyNames.LoggerName, logEntry.LoggerName },
            { EntityPropertyNames.Location, logEntry.Location ?? string.Empty },
            { EntityPropertyNames.Exception, logEntry.Exception ?? string.Empty },
            { EntityPropertyNames.Metadata, logEntry.Metadata ?? string.Empty }
        };

        try
        {
            await _tableClient.UpsertEntityAsync(entity, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new StorageException(
                "Failed to store log entry in Azure Table Storage.",
                ex);
        }
    }

    /// <summary>
    /// Retrieves logs from Azure Table Storage with filtering and pagination.
    /// </summary>
    /// <param name="query">The log query parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A tuple containing logs and continuation token for pagination.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when query is null.
    /// </exception>
    /// <exception cref="ValidationException">
    /// Thrown when query validation fails.
    /// </exception>
    /// <exception cref="StorageException">
    /// Thrown when retrieval fails.
    /// </exception>
    public async Task<(IReadOnlyList<LogEntry> Logs, string? ContinuationToken)>
        GetLogsAsync(
            LogQuery query,
            CancellationToken cancellationToken = default)
    {
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        var validationResult = query.Validate();
        if (!validationResult.IsValid)
        {
            throw new ValidationException(
                "Invalid log query.",
                validationResult.Errors);
        }

        try
        {
            var filterString = BuildFilterString(query.Filters);
            var asyncPagable = _tableClient.QueryAsync<TableEntity>(
                filterString,
                cancellationToken: cancellationToken);

            var logs = new List<LogEntry>();
            string? continuationToken = null;
            var count = 0;

            await foreach (var entity in asyncPagable.ConfigureAwait(false))
            {
                if (count >= query.PageSize)
                {
                    continuationToken = entity.RowKey;
                    break;
                }

                var logEntry = MapEntityToLogEntry(entity);
                logs.Add(logEntry);
                count++;
            }

            SortLogs(logs, query.OrderBy, query.Ascending);
            return (logs.AsReadOnly(), continuationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new StorageException(
                "Failed to retrieve logs from Azure Table Storage.",
                ex);
        }
    }

    /// <summary>
    /// Retrieves a single log entry by partition and row key.
    /// </summary>
    /// <param name="partitionKey">The partition key.</param>
    /// <param name="rowKey">The row key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The log entry if found; otherwise null.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when partition key or row key is invalid.
    /// </exception>
    /// <exception cref="StorageException">
    /// Thrown when retrieval fails.
    /// </exception>
    public async Task<LogEntry?> GetLogEntryAsync(
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken = default)
    {
        ValidatePartitionAndRowKeys(partitionKey, rowKey);

        try
        {
            var entity = await _tableClient.GetEntityAsync<TableEntity>(
                partitionKey,
                rowKey,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return MapEntityToLogEntry(entity.Value);
        }
        catch (Azure.RequestFailedException ex)
            when (ex.Status == Constants.HttpNotFoundStatusCode)
        {
            return null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new StorageException(
                "Failed to retrieve log entry from Azure Table Storage.",
                ex);
        }
    }

    /// <summary>
    /// Validates the connection string format.
    /// </summary>
    /// <param name="connectionString">The connection string to validate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when connection string is null or empty.
    /// </exception>
    private static void ValidateConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException(
                "Connection string cannot be null or empty.",
                nameof(connectionString));
        }
    }

    /// <summary>
    /// Validates the table name format.
    /// </summary>
    /// <param name="tableName">The table name to validate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when table name is null or empty.
    /// </exception>
    private static void ValidateTableName(string? tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(
                "Table name cannot be null or empty.",
                nameof(tableName));
        }
    }

    /// <summary>
    /// Validates partition and row keys.
    /// </summary>
    /// <param name="partitionKey">The partition key.</param>
    /// <param name="rowKey">The row key.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when either key is null or empty.
    /// </exception>
    private static void ValidatePartitionAndRowKeys(
        string? partitionKey,
        string? rowKey)
    {
        if (string.IsNullOrWhiteSpace(partitionKey))
        {
            throw new ArgumentException(
                "Partition key cannot be null or empty.",
                nameof(partitionKey));
        }

        if (string.IsNullOrWhiteSpace(rowKey))
        {
            throw new ArgumentException(
                "Row key cannot be null or empty.",
                nameof(rowKey));
        }
    }

    /// <summary>
    /// Expands Azurite connection string to full connection string.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>
    /// The expanded connection string or original if not Azurite.
    /// </returns>
    private static string ExpandAzuriteConnectionString(string connectionString)
    {
        if (connectionString.Equals(
            Constants.AzuriteConnectionString,
            StringComparison.OrdinalIgnoreCase))
        {
            return Constants.AzuriteFullConnectionString;
        }

        return connectionString;
    }

    /// <summary>
    /// Builds an OData filter string with injection prevention.
    /// </summary>
    /// <param name="filters">Dictionary of filter criteria.</param>
    /// <returns>
    /// The OData filter string or null if no filters provided.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when filter contains unsupported type.
    /// </exception>
    private static string? BuildFilterString(Dictionary<string, object>? filters)
    {
        if (filters == null || !filters.Any())
        {
            return null;
        }

        var conditions = new List<string>();

        foreach (var filter in filters)
        {
            ValidateFilterKey(filter.Key);
            var condition = BuildFilterCondition(filter.Key, filter.Value);
            conditions.Add(condition);
        }

        return string.Join(" and ", conditions);
    }

    /// <summary>
    /// Validates filter key to prevent OData injection.
    /// </summary>
    /// <param name="key">The filter key.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when key contains invalid characters.
    /// </exception>
    private static void ValidateFilterKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Filter key cannot be empty.", nameof(key));
        }

        // Only allow alphanumeric characters, underscores, and hyphens
        if (!key.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'))
        {
            throw new ArgumentException(
                "Filter key contains invalid characters.",
                nameof(key));
        }
    }

    /// <summary>
    /// Builds a single filter condition with type-specific formatting.
    /// </summary>
    /// <param name="key">The filter key.</param>
    /// <param name="value">The filter value.</param>
    /// <returns>The filter condition string.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when value type is not supported.
    /// </exception>
    private static string BuildFilterCondition(string key, object value)
    {
        return value switch
        {
            string strValue => BuildStringCondition(key, strValue),
            int intValue => $"{key} eq {intValue}",
            long longValue => $"{key} eq {longValue}L",
            double doubleValue => $"{key} eq {doubleValue}d",
            bool boolValue => $"{key} eq {boolValue.ToString().ToLowerInvariant()}",
            DateTimeOffset dateValue =>
                $"{key} ge datetime'{dateValue:O}' and {key} le " +
                $"datetime'{dateValue.AddDays(1):O}'",
            _ => throw new ArgumentException(
                $"Unsupported filter type '{value.GetType().Name}' for key '{key}'.",
                nameof(value))
        };
    }

    /// <summary>
    /// Builds a string filter condition with OData injection prevention.
    /// </summary>
    /// <param name="key">The filter key.</param>
    /// <param name="value">The filter value.</param>
    /// <returns>The filter condition string.</returns>
    private static string BuildStringCondition(string key, string value)
    {
        var escapedValue = EscapeODataValue(value);
        return $"{key} eq '{escapedValue}'";
    }

    /// <summary>
    /// Escapes single quotes in OData filter values (OWASP injection prevention).
    /// </summary>
    /// <param name="value">The value to escape.</param>
    /// <returns>The escaped value.</returns>
    private static string EscapeODataValue(string value)
    {
        return value.Replace("'", "''", StringComparison.Ordinal);
    }

    /// <summary>
    /// Maps a TableEntity to a LogEntry with null-safe property access.
    /// </summary>
    /// <param name="entity">The table entity.</param>
    /// <returns>The mapped log entry.</returns>
    private static LogEntry MapEntityToLogEntry(TableEntity entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        return new LogEntry
        {
            PartitionKey = entity.PartitionKey ?? string.Empty,
            RowKey = entity.RowKey ?? string.Empty,
            LoggerName = GetStringProperty(entity, EntityPropertyNames.LoggerName),
            Level = GetLogLevelProperty(entity, EntityPropertyNames.LogLevel),
            Message = GetStringProperty(entity, EntityPropertyNames.Message),
            TraceId = GetStringProperty(entity, EntityPropertyNames.TraceId),
            Location = GetStringProperty(entity, EntityPropertyNames.Location),
            Exception = GetStringProperty(entity, EntityPropertyNames.Exception),
            Metadata = GetStringProperty(entity, EntityPropertyNames.Metadata),
            Timestamp = DateTimeOffset.UtcNow.ToUniversalTime().ToString("o")
        };
    }

    /// <summary>
    /// Safely retrieves a string property from TableEntity.
    /// </summary>
    /// <param name="entity">The table entity.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The property value or empty string if not found.</returns>
    private static string GetStringProperty(TableEntity entity, string propertyName)
    {
        return entity.TryGetValue(propertyName, out var value) && value is string str
            ? str
            : string.Empty;
    }

    /// <summary>
    /// Safely retrieves and parses a LogLevel property from TableEntity.
    /// </summary>
    /// <param name="entity">The table entity.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>
    /// The parsed LogLevel or Information as default.
    /// </returns>
    private static LogLevel GetLogLevelProperty(
        TableEntity entity,
        string propertyName)
    {
        if (entity.TryGetValue(propertyName, out var value) && value is string str)
        {
            if (Enum.TryParse<LogLevel>(str, out var level))
            {
                return level;
            }
        }

        return LogLevel.Information;
    }

    /// <summary>
    /// Sorts log entries by specified field and order.
    /// </summary>
    /// <param name="logs">The logs to sort.</param>
    /// <param name="orderBy">The field to sort by.</param>
    /// <param name="ascending">Sort order (ascending if true).</param>
    private static void SortLogs(
        List<LogEntry> logs,
        string orderBy,
        bool ascending)
    {
        var sortFunc = (orderBy ?? "Timestamp") switch
        {
            "Timestamp" => (Func<LogEntry, IComparable>)(l =>
                l.Timestamp),
            "LogLevel" => l => l.Level,
            "TraceId" => l => l.TraceId ?? string.Empty,
            "LoggerName" => l => l.LoggerName,
            "Location" => l => l.Location ?? string.Empty,
            "Message" => l => l.Message,
            _ => l => l.Timestamp
        };

        logs.Sort((a, b) => ascending
            ? sortFunc(a).CompareTo(sortFunc(b))
            : sortFunc(b).CompareTo(sortFunc(a)));
    }

    /// <summary>
    /// Sanitizes input to prevent injection and enforce length limits.
    /// Removes control characters and truncates to max length.
    /// </summary>
    /// <param name="input">The input to sanitize.</param>
    /// <returns>The sanitized input.</returns>
    private static string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        // Remove control characters (OWASP: prevent injection)
        var sanitized = new string(input.Where(c => !char.IsControl(c))
            .ToArray());

        // Enforce maximum length
        return sanitized.Length > Constants.MaxMessageLength
            ? sanitized[..Constants.MaxMessageLength]
            : sanitized;
    }

    /// <summary>
    /// Creates the table if it doesn't already exist.
    /// Handles race conditions gracefully.
    /// </summary>
    private async Task CreateTableIfNotExistsAsync()
    {
        try
        {
            await _tableClient.CreateAsync().ConfigureAwait(false);
        }
        catch (Azure.RequestFailedException ex)
            when (ex.Status == Constants.HttpConflictStatusCode)
        {
            // Table already exists; this is expected
        }
    }
}
