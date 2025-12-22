# Configuration Guide

## Configuration Sources

The library supports multiple configuration sources:

1. **Application Settings** (`appsettings.json`)
2. **Azure App Configuration**
3. **Azure Key Vault**
4. **Environment Variables**
5. **Programmatic Configuration**

## Configuration Model

```csharp
public class AzureTableStorageLoggingConfiguration
{
    public string? ConnectionString { get; set; }      // Azure Storage connection string
    public string? TableName { get; set; }             // Table name for logs
    public string? LoggerName { get; set; }            // Logger identifier
    public string? DefaultTraceId { get; set; }        // Default trace ID
    public string? EnvironmentName { get; set; }       // Environment (Dev/Staging/Prod)
}
```

## Configuration Methods

### Method 1: Programmatic Configuration

```csharp
services.AddAzureTableStorageLogging(
    connectionString: "DefaultEndpointsProtocol=https;...",
    tableName: "logs",
    loggerName: "MyApplication",
    defaultTraceId: "optional-trace-id"
);
```

### Method 2: JSON Configuration (appsettings.json)

```json
{
  "AzureTableStorageLogging": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=myaccount;AccountKey=...;EndpointSuffix=core.windows.net",
    "TableName": "ApplicationLogs",
    "LoggerName": "MyApplication",
    "DefaultTraceId": "app-default-trace",
    "EnvironmentName": "Production"
  }
}
```

Load from configuration:

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var loggingConfig = configuration.GetAzureTableStorageLoggingConfiguration();

services.AddAzureTableStorageLogging(
    loggingConfig.ConnectionString!,
    loggingConfig.TableName!,
    loggingConfig.LoggerName!,
    loggingConfig.DefaultTraceId
);
```

### Method 3: Environment-Specific Configuration

```json
// appsettings.Development.json
{
  "AzureTableStorageLogging": {
    "ConnectionString": "UseDevelopmentStorage=true",
    "TableName": "DevLogs",
    "LoggerName": "MyApp-Dev"
  }
}

// appsettings.Production.json
{
  "AzureTableStorageLogging": {
    "ConnectionString": "DefaultEndpointsProtocol=https;...",
    "TableName": "AppLogs",
    "LoggerName": "MyApp-Production"
  }
}
```

Load with environment support:

```csharp
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{environment}.json")
    .Build();
```

## Azure Configuration Services

### Azure App Configuration

Azure App Configuration is ideal for managing application settings, feature flags, and configuration values.

**Setup:**

```csharp
var connectionString = "Endpoint=https://myappconfig.azureconfig.io;Id=...;Secret=...";

var configuration = new ConfigurationBuilder()
    .AddAzureAppConfiguration(connectionString)
    .Build();

var loggingConfig = configuration.GetAzureTableStorageLoggingConfiguration();
```

**Configure in Azure:**

1. Create an Azure App Configuration resource
2. Add key-value pairs:
   - `AzureTableStorageLogging:ConnectionString`
   - `AzureTableStorageLogging:TableName`
   - `AzureTableStorageLogging:LoggerName`
   - `AzureTableStorageLogging:EnvironmentName`

**Benefits:**

- Centralized configuration management
- Feature flag support
- Environment-specific configurations
- Configuration refresh without redeployment

### Azure Key Vault

Azure Key Vault is recommended for sensitive secrets like connection strings.

**Setup:**

```csharp
var keyVaultUri = "https://myvault.vault.azure.net";

var configuration = new ConfigurationBuilder()
    .AddAzureKeyVault(keyVaultUri)
    .Build();

// Retrieve secrets
var connectionString = configuration["AzureStorageConnectionString"];
```

**Configure in Azure:**

1. Create an Azure Key Vault resource
2. Add secrets:
   - `AzureStorageConnectionString`
   - `LogTableName`
   - `DefaultTraceId`

**Using Managed Identity:**

```csharp
// Automatically uses Managed Identity in Azure App Service
var credential = new DefaultAzureCredential();
var client = new SecretClient(new Uri(keyVaultUri), credential);
var secret = await client.GetSecretAsync("AzureStorageConnectionString");
var connectionString = secret.Value.Value;
```

### Combined Configuration

Recommended approach combining multiple sources:

```csharp
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
var keyVaultUri = "https://myvault.vault.azure.net";
var appConfigConnection = "Endpoint=https://myappconfig.azureconfig.io;...";

var configuration = new ConfigurationBuilder()
    // Start with defaults
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    // Override with Azure services
    .AddAzureAppConfiguration(appConfigConnection)
    .AddAzureKeyVault(keyVaultUri)
    // Allow environment variables to override
    .AddEnvironmentVariables()
    .Build();

var loggingConfig = configuration.GetAzureTableStorageLoggingConfiguration();

services.AddAzureTableStorageLogging(
    loggingConfig.ConnectionString!,
    loggingConfig.TableName!,
    loggingConfig.LoggerName!,
    loggingConfig.DefaultTraceId
);
```

## Environment Variables

```bash
# .env or set in environment
AZURE_STORAGE_CONNECTION_STRING=DefaultEndpointsProtocol=https;...
LOG_TABLE_NAME=ApplicationLogs
LOGGER_NAME=MyApplication
DEFAULT_TRACE_ID=app-trace
ASPNETCORE_ENVIRONMENT=Production
```

Load from environment:

```csharp
var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
var tableName = Environment.GetEnvironmentVariable("LOG_TABLE_NAME");
var loggerName = Environment.GetEnvironmentVariable("LOGGER_NAME");

services.AddAzureTableStorageLogging(
    connectionString!,
    tableName!,
    loggerName!
);
```

## Configuration Validation

Configuration is automatically validated when created:

```csharp
try
{
    var loggingConfig = configuration.GetAzureTableStorageLoggingConfiguration();
    // Configuration is valid
}
catch (InvalidOperationException ex)
{
    // Handle validation errors
    Console.WriteLine($"Configuration error: {ex.Message}");
}
```

Custom validation:

```csharp
var config = new AzureTableStorageLoggingConfiguration
{
    ConnectionString = "...",
    TableName = "logs",
    LoggerName = "MyApp"
};

var validationErrors = config.Validate();
if (validationErrors.Any())
{
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"Error: {error}");
    }
}
```

## Configuration Best Practices

1. **Never commit secrets** - Use Azure Key Vault for sensitive data
2. **Use Managed Identity** - Avoid storing credentials in code
3. **Environment-specific configs** - Separate settings for Dev/Staging/Prod
4. **Configuration validation** - Always validate before use
5. **Use strong naming** - Use clear, consistent naming conventions
6. **Document requirements** - Document all required configuration keys
7. **Provide defaults** - Have sensible defaults where possible
8. **Version your configs** - Track configuration changes

## Troubleshooting Configuration Issues

### Issue: Connection String Not Found

**Solution:** Ensure the connection string is set in one of the configuration sources:

```csharp
// Check if configuration exists
if (configuration["AzureTableStorageLogging:ConnectionString"] == null)
{
    throw new InvalidOperationException("ConnectionString is not configured");
}
```

### Issue: Key Vault Authentication Failed

**Solution:** Ensure proper permissions and authentication:

```csharp
// Ensure user/app has "Get" and "List" permissions on secrets
// Use DefaultAzureCredential which tries multiple authentication methods:
// 1. Environment variables
// 2. Managed Identity
// 3. Visual Studio authentication
// 4. Azure CLI authentication
```

### Issue: Table Name Invalid

**Solution:** Validate table name format:

```csharp
// Table names must:
// - Contain only letters, numbers
// - Start with a letter
// - Be 3-63 characters
var isValid = System.Text.RegularExpressions.Regex.IsMatch(
    tableName,
    @"^[a-zA-Z][a-zA-Z0-9]{2,62}$"
);
```
