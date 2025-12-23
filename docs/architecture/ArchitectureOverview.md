# Architecture Overview

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
│  (ASP.NET Core, Console Apps, Service Applications)        │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│            MasterZdran.Logging.AzureTableStorage                    │
│  ┌──────────────────────────────────────────────────────┐  │
│  │      AzureTableStorageLogger (Public API)           │  │
│  │  - DebugAsync() / InformationAsync() / etc.         │  │
│  │  - GetLogsAsync() / GetLogEntryAsync()              │  │
│  └──────────────────┬─────────────────────────────────┘  │
│                     │                                      │
│  ┌──────────────────▼─────────────────────────────────┐  │
│  │        ILogStorage Interface                       │  │
│  │  - StoreLogAsync()                                 │  │
│  │  - GetLogsAsync()                                  │  │
│  │  - GetLogEntryAsync()                              │  │
│  └──────────────────┬─────────────────────────────────┘  │
│                     │                                      │
│  ┌──────────────────▼─────────────────────────────────┐  │
│  │    AzureTableStorage (Implementation)              │  │
│  │  - Input Validation                                │  │
│  │  - Data Sanitization                               │  │
│  │  - Filter Building                                 │  │
│  │  - Error Handling                                  │  │
│  └──────────────────┬─────────────────────────────────┘  │
│                     │                                      │
│  ┌──────────────────▼─────────────────────────────────┐  │
│  │      Supporting Components                         │  │
│  │  - LogValidator                                    │  │
│  │  - LogEntry / LogQuery models                      │  │
│  │  - Custom Exceptions                               │  │
│  │  - ServiceCollectionExtensions (DI)                │  │
│  └─────────────────────────────────────────────────────┘  │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│        MasterZdran.Logging.Configuration (Configuration)             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │    Configuration Providers                          │  │
│  │  - AzureKeyVaultConfigurationProvider               │  │
│  │  - AzureAppConfigurationProvider                    │  │
│  │  - Standard JSON/Environment support                │  │
│  └──────────────────┬─────────────────────────────────┘  │
│                     │                                      │
│  ┌──────────────────▼─────────────────────────────────┐  │
│  │  ConfigurationBuilderExtensions                    │  │
│  │  - AddAzureKeyVault()                               │  │
│  │  - AddAzureAppConfiguration()                       │  │
│  │  - GetAzureTableStorageLoggingConfiguration()       │  │
│  └─────────────────────────────────────────────────────┘  │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│                Azure Cloud Services                         │
│  ┌──────────────────────────────────────────────────────┐  │
│  │     Azure Table Storage                             │  │
│  │  - Partition Key: LoggerName                        │  │
│  │  - Row Key: Timestamp + GUID                        │  │
│  │  - Columns: Level, Message, TraceId, Metadata, etc│  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │     Azure Key Vault                                 │  │
│  │  - Connection Strings                               │  │
│  │  - API Keys                                          │  │
│  │  - Secrets                                           │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │     Azure App Configuration                         │  │
│  │  - Feature Flags                                     │  │
│  │  - Settings                                          │  │
│  │  - Configuration Groups                              │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │     Azure Monitor / Application Insights            │  │
│  │  - OpenTelemetry Exporter                            │  │
│  │  - Distributed Tracing                               │  │
│  │  - Metrics Collection                                │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## Component Details

### 1. AzureTableStorageLogger

**Responsibilities:**

- Provide public logging API
- Automatic caller tracking
- Distributed tracing integration
- Exception enrichment
- Metadata serialization
- OpenTelemetry activity creation

**Key Methods:**

- `DebugAsync()` - Debug level logging
- `InformationAsync()` - Information level logging
- `WarningAsync()` - Warning level logging
- `ErrorAsync()` - Error level with exception tracking
- `CriticalAsync()` - Critical level with exception tracking
- `GetLogsAsync()` - Query with pagination
- `GetLogEntryAsync()` - Single entry retrieval

**Features:**

- Caller member name, file path, and line number tracking
- JSON serialization of metadata
- Activity/OpenTelemetry integration
- Automatic trace ID management
- Cancellation token support

### 2. ILogStorage Interface

**Contract Definition:**

```csharp
public interface ILogStorage
{
    Task StoreLogAsync(string partitionKey, string rowKey, LogEntry logEntry, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<LogEntry> Logs, string? ContinuationToken)> GetLogsAsync(LogQuery query, CancellationToken cancellationToken = default);
    Task<LogEntry?> GetLogEntryAsync(string partitionKey, string rowKey, CancellationToken cancellationToken = default);
}
```

**Purpose:**

- Define contract for storage implementations
- Enable testability through mocking
- Support alternative storage backends

### 3. AzureTableStorage Implementation

**Responsibilities:**

- Create/manage table connections
- Validate inputs
- Sanitize user data
- Build OData filters safely
- Map between models and Azure entities
- Handle pagination
- Exception handling and wrapping

**Security Features:**

- Input validation
- SQL injection prevention (OData injection)
- Control character removal
- Filter value escaping
- Size limits on messages

### 4. Configuration System

**Two-Part Design:**

**Part 1: Core Configuration (MasterZdran.Logging.AzureTableStorage)**

- `AzureTableStorageLoggingConfiguration` model
- Validation logic

**Part 2: Providers (MasterZdran.Logging.Configuration)**

- `AzureKeyVaultConfigurationProvider` - Azure Key Vault support
- `AzureAppConfigurationProvider` - Azure App Configuration support
- Extension methods for configuration builder

**Configuration Priority (top to bottom):**

1. Environment variables
2. Azure Key Vault
3. Azure App Configuration
4. JSON files (appsettings.json)
5. Programmatic defaults

### 5. Validation System

**ILogValidator Interface:**

- Contract for validation implementations
- LogValidator default implementation
- Fluent validation support

**Validation Points:**

- LogEntry validation (required fields, format)
- LogQuery validation (page size limits, field names)
- Configuration validation (required settings)

### 6. Dependency Injection

**ServiceCollectionExtensions:**

- `AddAzureTableStorageLogging()` - Register with string parameters
- `AddAzureTableStorageLogging()` - Register with factory function

**Registered Services:**

- `ILogValidator` → `LogValidator`
- `ILogStorage` → `AzureTableStorage`
- `AzureTableStorageLogger` → Singleton instance

## Data Model Design

### LogEntry Structure

```csharp
PartitionKey: string        // LoggerName (for partitioning)
RowKey: string              // Timestamp + GUID (for ordering)
Level: LogLevel             // DEBUG, INFO, WARNING, ERROR, CRITICAL
Message: string             // Log message (sanitized, max 4000 chars)
Timestamp: string           // ISO 8601 format
TraceId: string?            // Distributed trace identifier
LoggerName: string          // Logger name
Location: string            // Source location (Module:Line)
Metadata: string?           // JSON serialized metadata
```

### Azure Table Storage Mapping

```
Entity:
  PartitionKey: "MyApplication"
  RowKey: "20240101120000000000abcd1234"
  Timestamp: "2024-01-01T12:00:00.0000000Z"
  LogLevel: "Information"
  Message: "User logged in"
  TraceId: "trace-12345"
  LoggerName: "MyApplication"
  Location: "AuthService.Login:42"
  Metadata: "{\"user_id\": \"usr_123\", \"ip\": \"192.168.1.1\"}"
```

## Error Handling Strategy

### Exception Hierarchy

```
Exception
├── LoggingException (custom base)
│   ├── StorageException (storage failures)
│   └── ValidationException (validation errors)
└── Standard .NET Exceptions (network, timeout, etc.)
```

### Error Recovery

1. **Validation Errors** → Caught immediately, no retry
2. **Storage Errors** → Retry logic possible, graceful degradation
3. **Network Errors** → Timeout handling, circuit breaker pattern
4. **Authorization Errors** → Security audit logging

## Scalability Considerations

### Partitioning Strategy

- **Partition Key:** Logger name (allows parallel access)
- **Row Key:** Timestamp + GUID (ensures uniqueness, enables sorting)
- **Benefits:**
  - Different loggers don't contend for same partition
  - Time-based querying efficient
  - Pagination support

### Query Optimization

- Page size limit: 50-1000 records
- Continuation tokens for pagination
- Filter support for common fields
- Efficient sorting on indexed columns

### Performance Characteristics

- Write: O(1) - Direct table insert
- Single Read: O(1) - Direct key lookup
- Query: O(n) - Full partition scan with filtering
- Pagination: O(page_size) - Limited by page size

## Security Architecture

### Defense in Depth

1. **Input Layer:** Validation and sanitization
2. **Processing Layer:** Secure exception handling
3. **Storage Layer:** Azure encryption at rest
4. **Transport Layer:** HTTPS/TLS enforcement
5. **Identity Layer:** Managed Identity / RBAC

### Trust Boundaries

```
     Application      (Trusted)
            │
            ▼
     Logging Library  (Validates/Sanitizes)
            │
            ▼
    Azure SDK Client  (Encrypts)
            │
            ▼
   Azure Services     (Encrypted, RBAC)
```

## Testing Architecture

### Test Levels

1. **Unit Tests**

   - Validation logic
   - Exception handling
   - Model mapping

2. **Integration Tests**

   - Azure Storage operations
   - Configuration loading
   - End-to-end logging

3. **Security Tests**
   - Injection prevention
   - Input sanitization
   - Exception message safety

### Mocking Strategy

- Mock `ILogStorage` for logger tests
- Mock Azure SDK clients for storage tests
- Mock configuration providers
- Real Azure Emulator for integration tests

## Deployment Architecture

### Development

```
Local Machine
├── Azure Storage Emulator
├── JSON Configuration
└── Console/Test Apps
```

### Production

```
Azure Cloud
├── Azure Table Storage
├── Azure Key Vault (secrets)
├── Azure App Configuration (settings)
├── Managed Identity (auth)
├── Application Insights (monitoring)
└── App Service/Container Instance
```

## Performance Monitoring

### Metrics to Track

- Log write latency (p50, p95, p99)
- Query response time
- Error rate by type
- Storage usage by logger
- API call frequency

### OpenTelemetry Instrumentation

- Activity creation for all log operations
- Duration tracking
- Exception recording
- Custom tags for metadata
