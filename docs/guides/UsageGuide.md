# Usage Guide

Practical guide to using MZ.Logging.AzureTableStorage in various scenarios.

---

## Table of Contents

1. [Getting Started](#getting-started)
2. [ASP.NET Core Integration](#aspnet-core-integration)
3. [Console Applications](#console-applications)
4. [Worker Services](#worker-services)
5. [Azure Functions](#azure-functions)
6. [Structured Logging](#structured-logging)
7. [Exception Handling](#exception-handling)
8. [Distributed Tracing](#distributed-tracing)
9. [Querying Logs](#querying-logs)
10. [Performance Optimization](#performance-optimization)
11. [Local Development](#local-development)
12. [Production Deployment](#production-deployment)

---

## Getting Started

### Prerequisites

- .NET 8.0 or .NET 7.0 SDK
- Azure Storage Account (or Azurite for local development)
- Basic understanding of async/await and dependency injection

### Installation

```bash
# Core logging library
dotnet add package MZ.Logging.AzureTableStorage

# Configuration helpers (optional)
dotnet add package MZ.Logging.Configuration
```

---

## ASP.NET Core Integration

### Program.cs Setup

```csharp
using MZ.Logging.AzureTableStorage;

var builder = WebApplication.CreateBuilder(args);

// Add Azure Table Storage logging
builder.Services.AddAzureTableStorageLogging(
    connectionString: builder.Configuration["AzureTableStorage:ConnectionString"]!,
    tableName: "logs",
    loggerName: builder.Environment.ApplicationName
);

// Add controllers and other services
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();
```

### Controller Usage

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ILogger<OrdersController> _logger;
    private readonly IOrderService _orderService;

    public OrdersController(
        ILogger<OrdersController> logger,
        IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        _logger.LogInformation(
            "Creating order for customer {CustomerId} with {ItemCount} items",
            request.CustomerId,
            request.Items.Count
        );

        try
        {
            var order = await _orderService.CreateAsync(request);

            _logger.LogInformation(
                "Order {OrderId} created successfully",
                order.Id
            );

            return Ok(order);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(
                ex,
                "Order validation failed for customer {CustomerId}",
                request.CustomerId
            );
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create order for customer {CustomerId}",
                request.CustomerId
            );
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(string id)
    {
        _logger.LogDebug("Retrieving order {OrderId}", id);

        var order = await _orderService.GetAsync(id);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", id);
            return NotFound();
        }

        return Ok(order);
    }
}
```

### Middleware Integration

```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = Guid.NewGuid().ToString();

        _logger.LogInformation(
            "Incoming {Method} request to {Path} (RequestId: {RequestId})",
            context.Request.Method,
            context.Request.Path,
            requestId
        );

        var sw = Stopwatch.StartNew();

        try
        {
            await _next(context);

            sw.Stop();

            _logger.LogInformation(
                "Completed {Method} {Path} with {StatusCode} in {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds
            );
        }
        catch (Exception ex)
        {
            sw.Stop();

            _logger.LogError(
                ex,
                "Request {Method} {Path} failed after {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                sw.ElapsedMilliseconds
            );

            throw;
        }
    }
}

// Register in Program.cs
app.UseMiddleware<RequestLoggingMiddleware>();
```

---

## Console Applications

### Basic Console App

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MZ.Logging.AzureTableStorage;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup DI container
        var services = new ServiceCollection();

        // Add logging
        services.AddAzureTableStorageLogging(
            "UseDevelopmentStorage=true",
            "logs",
            "ConsoleApp"
        );

        // Add your services
        services.AddTransient<DataProcessor>();

        var serviceProvider = services.BuildServiceProvider();

        // Get logger
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Application started");

        try
        {
            var processor = serviceProvider.GetRequiredService<DataProcessor>();
            await processor.ProcessAsync();

            logger.LogInformation("Application completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Application failed");
            Environment.Exit(1);
        }
    }
}

class DataProcessor
{
    private readonly ILogger<DataProcessor> _logger;

    public DataProcessor(ILogger<DataProcessor> logger)
    {
        _logger = logger;
    }

    public async Task ProcessAsync()
    {
        _logger.LogInformation("Starting data processing");

        // Your processing logic
        await Task.Delay(1000);

        _logger.LogInformation("Data processing completed");
    }
}
```

---

## Worker Services

### Background Worker

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class DataSyncWorker : BackgroundService
{
    private readonly ILogger<DataSyncWorker> _logger;
    private readonly IDataSyncService _syncService;

    public DataSyncWorker(
        ILogger<DataSyncWorker> logger,
        IDataSyncService syncService)
    {
        _logger = logger;
        _syncService = syncService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Data sync worker starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("Starting sync cycle");

                await _syncService.SyncAsync(stoppingToken);

                _logger.LogInformation("Sync cycle completed");

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Sync worker stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sync cycle failed");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        _logger.LogInformation("Data sync worker stopped");
    }
}

// Program.cs
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAzureTableStorageLogging(
    builder.Configuration["AzureTableStorage:ConnectionString"]!,
    "logs",
    "DataSyncWorker"
);

builder.Services.AddHostedService<DataSyncWorker>();
builder.Services.AddTransient<IDataSyncService, DataSyncService>();

var host = builder.Build();
host.Run();
```

---

## Azure Functions

### HTTP Trigger Function

```csharp
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

public class OrderFunction
{
    private readonly ILogger<OrderFunction> _logger;
    private readonly IOrderService _orderService;

    public OrderFunction(
        ILogger<OrderFunction> logger,
        IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    [Function("CreateOrder")]
    public async Task<HttpResponseData> CreateOrder(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("Processing CreateOrder request");

        try
        {
            var order = await req.ReadFromJsonAsync<CreateOrderRequest>();

            if (order == null)
            {
                _logger.LogWarning("Invalid request body");
                var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                return badResponse;
            }

            var result = await _orderService.CreateAsync(order);

            _logger.LogInformation("Order {OrderId} created", result.Id);

            var response = req.CreateResponse(System.Net.HttpStatusCode.Created);
            await response.WriteAsJsonAsync(result);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create order");
            var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            return errorResponse;
        }
    }
}

// Program.cs (Azure Functions Isolated)
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddAzureTableStorageLogging(
            Environment.GetEnvironmentVariable("AzureTableStorage:ConnectionString")!,
            "logs",
            "OrderFunction"
        );

        services.AddTransient<IOrderService, OrderService>();
    })
    .Build();

host.Run();
```

---

## Structured Logging

### Using Message Templates

```csharp
// Good - structured with placeholders
_logger.LogInformation(
    "User {UserId} placed order {OrderId} with total {Amount:C}",
    userId,
    orderId,
    totalAmount
);

// Bad - string interpolation loses structure
_logger.LogInformation($"User {userId} placed order {orderId} with total {totalAmount:C}");
```

### Rich Metadata with Direct Logger

```csharp
var logger = serviceProvider.GetRequiredService<AzureTableStorageLogger>();

await logger.InformationAsync(
    "Payment processed",
    traceId: Activity.Current?.Id,
    metadata: new Dictionary<string, object>
    {
        { "orderId", orderId },
        { "userId", userId },
        { "amount", amount },
        { "currency", "USD" },
        { "paymentMethod", "CreditCard" },
        { "transactionId", transactionId },
        { "processingTimeMs", processingTime.TotalMilliseconds }
    }
);
```

### Logging Scopes

```csharp
using (_logger.BeginScope(new Dictionary<string, object>
{
    { "OrderId", orderId },
    { "UserId", userId }
}))
{
    _logger.LogInformation("Processing order");
    _logger.LogDebug("Validating items");
    _logger.LogInformation("Order validated");
    // All logs in this scope will include OrderId and UserId
}
```

---

## Exception Handling

### Logging Exceptions

```csharp
try
{
    await ProcessAsync();
}
catch (ValidationException ex)
{
    _logger.LogWarning(ex, "Validation failed for {EntityId}", entityId);
}
catch (DatabaseException ex)
{
    _logger.LogError(ex, "Database error for operation {Operation}", operation);
}
catch (Exception ex)
{
    _logger.LogCritical(ex, "Unexpected error in {Method}", nameof(ProcessAsync));
    throw;
}
```

### Exception with Metadata

```csharp
var logger = serviceProvider.GetRequiredService<AzureTableStorageLogger>();

try
{
    await RiskyOperationAsync();
}
catch (Exception ex)
{
    await logger.ErrorAsync(
        "Operation failed",
        exception: ex,
        metadata: new Dictionary<string, object>
        {
            { "operationId", operationId },
            { "retryCount", retryCount },
            { "elapsedMs", elapsed.TotalMilliseconds }
        }
    );
}
```

---

## Distributed Tracing

### Automatic Trace ID Capture

```csharp
using System.Diagnostics;

// Start an activity
using var activity = new Activity("ProcessOrder").Start();

_logger.LogInformation("Processing order {OrderId}", orderId);
// TraceId automatically captured from Activity.Current
```

### Manual Trace ID

```csharp
var traceId = HttpContext.TraceIdentifier;

_logger.LogInformation(
    "Request {RequestId} processing",
    traceId
);

// Or with direct logger
await logger.InformationAsync(
    "Processing request",
    traceId: traceId
);
```

### Microservices Correlation

```csharp
// Service A
var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
_logger.LogInformation("Calling Service B with trace {TraceId}", traceId);

var request = new HttpRequestMessage(HttpMethod.Post, "https://serviceb/api/process");
request.Headers.Add("X-Trace-Id", traceId);
await httpClient.SendAsync(request);

// Service B
var traceId = Request.Headers["X-Trace-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString();
_logger.LogInformation("Received request from Service A with trace {TraceId}", traceId);
```

---

## Querying Logs

### Basic Query

```csharp
var logger = serviceProvider.GetRequiredService<AzureTableStorageLogger>();

var query = new LogQuery
{
    PageSize = 50,
    OrderBy = "Timestamp",
    Ascending = false
};

var (logs, _) = await logger.GetLogsAsync(query);

foreach (var log in logs)
{
    Console.WriteLine($"[{log.Level}] {log.Timestamp}: {log.Message}");
}
```

### Filtered Query

```csharp
var query = new LogQuery
{
    PageSize = 100,
    OrderBy = "Timestamp",
    Ascending = false,
    Filters = new Dictionary<string, object>
    {
        { "LogLevel", "Error" },
        { "LoggerName", "OrderService" }
    }
};

var (errorLogs, _) = await logger.GetLogsAsync(query);
```

### Pagination

```csharp
var query = new LogQuery
{
    PageSize = 50,
    OrderBy = "Timestamp"
};

var allLogs = new List<LogEntry>();
string? continuationToken = null;

do
{
    query.ContinuationToken = continuationToken;
    var (logs, nextToken) = await logger.GetLogsAsync(query);

    allLogs.AddRange(logs);
    continuationToken = nextToken;

} while (continuationToken != null);

Console.WriteLine($"Retrieved {allLogs.Count} total logs");
```

### Query by Trace ID

```csharp
var query = new LogQuery
{
    Filters = new Dictionary<string, object>
    {
        { "TraceId", traceId }
    },
    OrderBy = "Timestamp",
    Ascending = true
};

var (traceLogs, _) = await logger.GetLogsAsync(query);

// Show complete trace
foreach (var log in traceLogs)
{
    Console.WriteLine($"{log.Timestamp} [{log.Level}] {log.Message}");
}
```

---

## Performance Optimization

### Use Appropriate Log Levels

```csharp
// In development
#if DEBUG
_logger.LogDebug("Detailed diagnostics: {Data}", data);
#endif

// In production - avoid excessive debug logs
if (_logger.IsEnabled(LogLevel.Debug))
{
    var expensiveData = ComputeExpensiveData();
    _logger.LogDebug("Debug info: {Data}", expensiveData);
}
```

### Async All the Way

```csharp
// Good - async all the way
public async Task ProcessAsync()
{
    await _logger.InformationAsync("Processing started");
    await DoWorkAsync();
    await _logger.InformationAsync("Processing completed");
}

// Bad - blocking on async
public void Process()
{
    _logger.InformationAsync("Processing").Wait(); // DON'T DO THIS
}
```

### Batch Operations

```csharp
// Process multiple items with periodic logging
var items = GetItems();
var batchSize = 100;
var processed = 0;

foreach (var batch in items.Chunk(batchSize))
{
    await ProcessBatchAsync(batch);
    processed += batch.Length;

    await logger.InformationAsync(
        "Processed {Count}/{Total} items",
        metadata: new Dictionary<string, object>
        {
            { "processed", processed },
            { "total", items.Count }
        }
    );
}
```

---

## Local Development

### Using Azurite

```bash
# Install Azurite
npm install -g azurite

# Start table storage emulator
azurite-table --location ./azurite --debug ./azurite/debug.log
```

### Configuration

```json
{
  "AzureTableStorageLogging": {
    "ConnectionString": "UseDevelopmentStorage=true",
    "TableName": "logs",
    "LoggerName": "MyApp-Dev"
  }
}
```

### Viewing Logs Locally

```csharp
// Query recent logs
var query = new LogQuery
{
    PageSize = 20,
    OrderBy = "Timestamp",
    Ascending = false
};

var (logs, _) = await logger.GetLogsAsync(query);

foreach (var log in logs)
{
    var color = log.Level switch
    {
        LogLevel.Error => ConsoleColor.Red,
        LogLevel.Warning => ConsoleColor.Yellow,
        LogLevel.Information => ConsoleColor.Green,
        _ => ConsoleColor.Gray
    };

    Console.ForegroundColor = color;
    Console.WriteLine($"[{log.Level}] {log.Message}");
    Console.ResetColor();
}
```

---

## Production Deployment

### Secure Configuration

```csharp
// Use Azure Key Vault
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddAzureKeyVault(
        new Uri("https://myvault.vault.azure.net"),
        new DefaultAzureCredential()
    )
    .Build();
```

### Managed Identity

```csharp
// App Service or Azure VM with Managed Identity
var credential = new DefaultAzureCredential();
// Azure SDK automatically uses Managed Identity
```

### Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<LoggingHealthCheck>("logging");

public class LoggingHealthCheck : IHealthCheck
{
    private readonly AzureTableStorageLogger _logger;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _logger.DebugAsync("Health check", cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Logging failed", ex);
        }
    }
}
```

### Monitoring

```csharp
// Add Application Insights for additional monitoring
builder.Services.AddApplicationInsightsTelemetry();

// Logs will be available in both Azure Table Storage and Application Insights
```

---

For more information, see:

- [API Reference](../api/ApiReference.md)
- [Security Best Practices](SecurityBestPractices.md)
- [Configuration Guide](Configuration.md)
