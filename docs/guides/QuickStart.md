# Quick Start Guide

## Installation

### Using NuGet Package Manager

```bash
Install-Package MZ.Logging.AzureTableStorage
Install-Package MZ.Logging.Configuration
```

### Using .NET CLI

```bash
dotnet add package MZ.Logging.AzureTableStorage
dotnet add package MZ.Logging.Configuration
```

## 5-Minute Setup

### Step 1: Configure Services

In your `Startup.cs` or `Program.cs`:

```csharp
var services = new ServiceCollection();

// Register Azure Table Storage Logger
services.AddAzureTableStorageLogging(
    connectionString: "DefaultEndpointsProtocol=https;AccountName=myaccount;AccountKey=...;EndpointSuffix=core.windows.net",
    tableName: "AppLogs",
    loggerName: "MyApplication"
);

var serviceProvider = services.BuildServiceProvider();
```

### Step 2: Get Logger Instance

```csharp
var logger = serviceProvider.GetRequiredService<AzureTableStorageLogger>();
```

### Step 3: Start Logging

```csharp
// Simple log
await logger.InformationAsync("Application started");

// With metadata
await logger.InformationAsync(
    "User logged in",
    metadata: new Dictionary<string, object>
    {
        { "user_id", "user123" },
        { "ip_address", "192.168.1.1" }
    }
);

// With exception handling
try
{
    await SomeOperationAsync();
}
catch (Exception ex)
{
    await logger.ErrorAsync(
        "Operation failed",
        exception: ex
    );
}
```

## Common Patterns

### Pattern 1: Request Tracing

```csharp
public async Task ProcessRequestAsync(HttpRequest request)
{
    var traceId = request.HttpContext.TraceIdentifier;

    await logger.InformationAsync(
        "Processing request",
        traceId: traceId,
        metadata: new Dictionary<string, object>
        {
            { "method", request.Method },
            { "path", request.Path },
            { "remote_ip", request.HttpContext.Connection.RemoteIpAddress?.ToString() }
        }
    );
}
```

### Pattern 2: Performance Monitoring

```csharp
public async Task<T> ExecuteWithLoggingAsync<T>(
    string operationName,
    Func<Task<T>> operation,
    string traceId)
{
    var stopwatch = Stopwatch.StartNew();

    try
    {
        var result = await operation();
        stopwatch.Stop();

        await logger.InformationAsync(
            $"{operationName} completed",
            traceId: traceId,
            metadata: new Dictionary<string, object>
            {
                { "duration_ms", stopwatch.ElapsedMilliseconds },
                { "status", "success" }
            }
        );

        return result;
    }
    catch (Exception ex)
    {
        stopwatch.Stop();

        await logger.ErrorAsync(
            $"{operationName} failed",
            exception: ex,
            traceId: traceId,
            metadata: new Dictionary<string, object>
            {
                { "duration_ms", stopwatch.ElapsedMilliseconds },
                { "status", "failed" }
            }
        );

        throw;
    }
}
```

### Pattern 3: Dependency Injection in Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AzureTableStorageLogger _logger;
    private readonly IUserService _userService;

    public UsersController(AzureTableStorageLogger logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserAsync(CreateUserRequest request)
    {
        var traceId = HttpContext.TraceIdentifier;

        try
        {
            var user = await _userService.CreateUserAsync(request);

            await _logger.InformationAsync(
                "User created successfully",
                traceId: traceId,
                metadata: new Dictionary<string, object>
                {
                    { "user_id", user.Id },
                    { "email", user.Email }
                }
            );

            return Created($"api/users/{user.Id}", user);
        }
        catch (ValidationException ex)
        {
            await _logger.WarningAsync(
                "User creation validation failed",
                traceId: traceId,
                metadata: new Dictionary<string, object>
                {
                    { "errors", string.Join(", ", ex.Errors) }
                }
            );

            return BadRequest(ex.Errors);
        }
        catch (Exception ex)
        {
            await _logger.CriticalAsync(
                "Unexpected error creating user",
                exception: ex,
                traceId: traceId
            );

            return StatusCode(500, "Internal server error");
        }
    }
}
```

### Pattern 4: Batch Logging

```csharp
public async Task LogMultipleEventsAsync(IEnumerable<LogEvent> events, string traceId)
{
    var tasks = events.Select(async e =>
    {
        var metadata = new Dictionary<string, object>
        {
            { "event_type", e.EventType },
            { "event_id", e.Id }
        };

        if (e.Severity > 3)
        {
            await logger.ErrorAsync(e.Message, traceId: traceId, metadata: metadata);
        }
        else
        {
            await logger.InformationAsync(e.Message, traceId: traceId, metadata: metadata);
        }
    });

    await Task.WhenAll(tasks);
}
```

## Next Steps

- See [Configuration Guide](Configuration.md) for advanced configuration
- See [Security Best Practices](SecurityBestPractices.md) for securing your logs
- See [API Reference](../api/ApiReference.md) for complete API documentation
