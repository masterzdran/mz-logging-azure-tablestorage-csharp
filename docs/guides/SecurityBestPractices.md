# Security Best Practices

This guide covers OWASP Top 10 and other security considerations for using MZ.Logging.AzureTableStorage.

## 1. Secure Secret Management (CWE-798)

### DO: Use Azure Key Vault

```csharp
// ✓ Secure: Using Azure Key Vault
var keyVaultUri = "https://myvault.vault.azure.net";
var configuration = new ConfigurationBuilder()
    .AddAzureKeyVault(keyVaultUri)
    .Build();

var connectionString = configuration["AzureStorageConnectionString"];
```

### DON'T: Hardcode Secrets

```csharp
// ✗ INSECURE: Never do this!
var connectionString = "DefaultEndpointsProtocol=https;AccountName=myaccount;AccountKey=supersecretkey123...";
```

### DON'T: Commit to Source Control

```bash
# ✗ INSECURE: Don't commit .env or config files with secrets
.env
appsettings.Production.json
```

### DO: Use .gitignore

```bash
# ✓ Secure: Ignore sensitive files
.env
*.local.json
appsettings.Production.json
appsettings.*.json
```

## 2. Input Validation and Injection Prevention (CWE-20, CWE-89)

### DO: Validate All Inputs

```csharp
// ✓ Secure: Input validation is built-in
var logEntry = new LogEntry
{
    PartitionKey = partitionKey,
    RowKey = rowKey,
    Message = message,
    // ...
};

var validationResult = logEntry.Validate();
if (!validationResult.IsValid)
{
    throw new ValidationException("Invalid log entry", validationResult.Errors);
}
```

### DO: Use Query Validation

```csharp
// ✓ Secure: Query validation prevents injection
var query = new LogQuery
{
    PageSize = 50,
    OrderBy = "Timestamp",
    Filters = new Dictionary<string, object>
    {
        { "LogLevel", "Error" }
    }
};

var validationResult = query.Validate();
if (!validationResult.IsValid)
{
    // Handle validation errors
}
```

### DON'T: Trust User Input

```csharp
// ✗ INSECURE: Never trust raw user input
var userFilter = Request.Query["filter"].ToString();
var query = new LogQuery { /* ... */ };
// This could contain injection attempts
```

## 3. Secure Error Handling (CWE-209, CWE-215)

### DO: Handle Exceptions Securely

```csharp
// ✓ Secure: No sensitive data in exceptions
try
{
    await logger.InformationAsync("Processing request");
}
catch (StorageException ex)
{
    // Log the error safely
    await logger.ErrorAsync(
        "Failed to store log",
        exception: ex
    );
    // Don't expose exception details to users
    return StatusCode(500, "An error occurred");
}
```

### DON'T: Expose Stack Traces

```csharp
// ✗ INSECURE: Don't expose stack traces to users
catch (Exception ex)
{
    return BadRequest(new { error = ex.ToString() }); // INSECURE!
}
```

### DO: Log Full Details Internally

```csharp
// ✓ Secure: Log full details internally
catch (Exception ex)
{
    await logger.CriticalAsync(
        "Unexpected error",
        exception: ex,
        metadata: new Dictionary<string, object>
        {
            { "request_id", requestId },
            { "user_id", userId },
            { "operation", "ProcessPayment" }
        }
    );
    // Return safe message to user
    return StatusCode(500, "Operation failed");
}
```

## 4. Sensitive Data Protection (CWE-200)

### DO: Sanitize PII in Logs

```csharp
// ✓ Secure: Sanitize personally identifiable information
public static string MaskEmail(string email)
{
    var parts = email.Split('@');
    var masked = parts[0].Substring(0, 2) + "*****";
    return $"{masked}@{parts[1]}";
}

await logger.InformationAsync(
    "User login",
    metadata: new Dictionary<string, object>
    {
        { "email", MaskEmail("user@example.com") } // Masked: "us*****@example.com"
    }
);
```

### DO: Avoid Logging Passwords

```csharp
// ✓ Secure: Never log sensitive credentials
var metadata = new Dictionary<string, object>
{
    { "username", username },
    // ✗ NEVER: { "password", password }
};

await logger.InformationAsync("Login attempt", metadata: metadata);
```

### DO: Use Data Classification

```csharp
public static Dictionary<string, object> ClassifyMetadata(Dictionary<string, object> data)
{
    var sanitized = new Dictionary<string, object>();

    foreach (var item in data)
    {
        // Only include non-sensitive fields
        if (!IsSernsitiveField(item.Key))
        {
            sanitized[item.Key] = item.Value;
        }
    }

    return sanitized;
}

private static bool IsSensitiveField(string fieldName)
{
    var sensitiveFields = new[] { "password", "token", "secret", "key", "ssn", "credit_card" };
    return sensitiveFields.Contains(fieldName.ToLower());
}
```

## 5. Authentication and Authorization (CWE-287, CWE-639)

### DO: Use Managed Identity

```csharp
// ✓ Secure: Managed Identity in Azure
var credential = new DefaultAzureCredential();
var tableServiceClient = new TableServiceClient(
    new Uri("https://myaccount.table.core.windows.net"),
    credential
);
```

### DO: Implement RBAC

```csharp
// ✓ Secure: Role-Based Access Control
// Assign Azure roles to service principal:
// - "Storage Blob Data Contributor"
// - "Storage Table Data Contributor"
// Avoid "Storage Account Key Operator Service Role"
```

### DON'T: Share Connection Strings

```csharp
// ✗ INSECURE: Connection strings should not be shared
var sharedConnectionString = "DefaultEndpointsProtocol=https;AccountName=myaccount;...";
// This could be compromised
```

## 6. Data Encryption (CWE-327)

### DO: Use HTTPS

```csharp
// ✓ Secure: Azure SDKs enforce HTTPS by default
// Connection strings with 'DefaultEndpointsProtocol=https' use TLS
```

### DO: Enable Azure Storage Encryption

```
Azure Portal → Storage Account → Encryption
- Enable encryption at rest (default: enabled)
- Enable infrastructure encryption (recommended)
```

## 7. Access Control and Audit Logging

### DO: Enable Azure Auditing

```
Azure Portal → Storage Account → Settings → Diagnostic settings
- Enable "StorageRead", "StorageWrite", "StorageDelete"
- Send to Log Analytics or Event Hub
```

### DO: Track Access

```csharp
// ✓ Secure: Log all logging operations
await logger.InformationAsync(
    "Logs accessed",
    metadata: new Dictionary<string, object>
    {
        { "requester_id", userId },
        { "requester_role", userRole },
        { "query_type", "GetLogs" },
        { "filter_count", filterCount },
        { "records_returned", recordCount }
    }
);
```

## 8. Resource Management and DoS Prevention

### DO: Implement Rate Limiting

```csharp
// ✓ Secure: Rate limiting for log retrieval
public class RateLimitedLogger
{
    private readonly AzureTableStorageLogger _logger;
    private readonly SemaphoreSlim _semaphore = new(100); // Max 100 concurrent operations

    public async Task LogWithRateLimitAsync(
        string message,
        Dictionary<string, object>? metadata = null)
    {
        await _semaphore.WaitAsync();
        try
        {
            await _logger.InformationAsync(message, metadata: metadata);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```

### DO: Set Query Limits

```csharp
// ✓ Secure: Limit query results
var query = new LogQuery
{
    PageSize = 100,  // Limit to 100 items max
    Filters = new Dictionary<string, object>
    {
        { "LogLevel", "Error" }
    }
};

// Pagination prevents retrieving all records at once
var (logs, continuationToken) = await logger.GetLogsAsync(query);
```

### DO: Use Cancellation Tokens

```csharp
// ✓ Secure: Support cancellation for long operations
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    await logger.InformationAsync(
        "Processing large query",
        cancellationToken: cts.Token
    );
}
catch (OperationCanceledException)
{
    // Timeout protection
}
```

## 9. Testing Security

### DO: Test Input Validation

```csharp
[Fact]
public async Task LogEntry_InvalidPartitionKey_ThrowsValidationException()
{
    var logEntry = new LogEntry
    {
        PartitionKey = "", // Invalid
        RowKey = "validKey",
        Message = "Test",
        LoggerName = "Test",
        Location = "Test",
        Timestamp = DateTime.UtcNow.ToString("O")
    };

    var result = logEntry.Validate();
    Assert.False(result.IsValid);
    Assert.Contains("PartitionKey", string.Join(" ", result.Errors));
}
```

### DO: Test Exception Handling

```csharp
[Fact]
public async Task StoreLog_ConnectionFailure_ThrowsStorageException()
{
    var mockStorage = new Mock<ILogStorage>();
    mockStorage.Setup(s => s.StoreLogAsync(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<LogEntry>(),
        It.IsAny<CancellationToken>()))
        .ThrowsAsync(new HttpRequestException("Connection failed"));

    var logger = new AzureTableStorageLogger(mockStorage.Object, "TestApp");

    await Assert.ThrowsAsync<StorageException>(
        () => logger.InformationAsync("Test")
    );
}
```

## 10. Compliance and Audit

### DO: Implement Audit Logging

```csharp
// ✓ Secure: Audit trail for sensitive operations
await logger.InformationAsync(
    "Configuration accessed",
    metadata: new Dictionary<string, object>
    {
        { "event_type", "ConfigurationAccess" },
        { "user_id", userId },
        { "timestamp", DateTime.UtcNow },
        { "source_ip", sourceIp },
        { "changes", "none" } // Track what was changed
    }
);
```

### DO: Enable Compliance Features

```
Compliance Requirements:
- GDPR: Data retention policies, right to be forgotten
- HIPAA: Encryption, access logging, audit trails
- PCI-DSS: Encryption of cardholder data, secure deletion
- SOC 2: Access controls, audit logging
```

## Security Checklist

- [ ] Secrets stored in Azure Key Vault
- [ ] No hardcoded credentials
- [ ] Input validation enabled
- [ ] HTTPS/TLS enforced
- [ ] Managed Identity used (not connection strings)
- [ ] Error messages don't expose details
- [ ] PII sanitized in logs
- [ ] Rate limiting implemented
- [ ] Audit logging enabled
- [ ] Access controls configured
- [ ] Regular security updates applied
- [ ] Penetration testing completed

## Reporting Security Vulnerabilities

If you discover a security vulnerability, please email security@example.com instead of using the issue tracker.
