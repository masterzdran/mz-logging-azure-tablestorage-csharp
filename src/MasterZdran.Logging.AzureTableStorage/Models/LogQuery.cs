namespace MasterZdran.Logging.AzureTableStorage;

/// <summary>
/// Represents a query for retrieving logs with filtering and pagination.
/// </summary>
public class LogQuery
{
    /// <summary>
    /// Gets or sets the page size for pagination.
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Gets or sets the continuation token for pagination.
    /// </summary>
    public string? ContinuationToken { get; set; }

    /// <summary>
    /// Gets or sets the field to order results by.
    /// </summary>
    public string OrderBy { get; set; } = "Timestamp";

    /// <summary>
    /// Gets or sets whether to order in ascending order.
    /// </summary>
    public bool Ascending { get; set; }

    /// <summary>
    /// Gets or sets filter conditions.
    /// </summary>
    public Dictionary<string, object> Filters { get; set; } = new();

    /// <summary>
    /// Validates the query parameters.
    /// </summary>
    /// <returns>Validation result with errors if any.</returns>
    public ValidationResult Validate()
    {
        var errors = new List<string>();

        if (PageSize <= 0)
            errors.Add("PageSize must be greater than 0.");

        if (PageSize > 1000)
            errors.Add("PageSize cannot exceed 1000.");

        if (string.IsNullOrWhiteSpace(OrderBy))
            errors.Add("OrderBy field cannot be empty.");

        var validFields = new[] { "Timestamp", "LogLevel", "TraceId", "LoggerName", "Location", "Message" };
        if (!validFields.Contains(OrderBy))
            errors.Add($"OrderBy field must be one of: {string.Join(", ", validFields)}");

        return new ValidationResult { IsValid = !errors.Any(), Errors = errors };
    }
}

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets or sets whether validation passed.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets validation error messages.
    /// </summary>
    public List<string> Errors { get; set; } = new();
}
