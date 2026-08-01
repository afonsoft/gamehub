---
name: performance-optimization
license: UNLICENSED
description: Use when identifying performance bottlenecks, optimizing queries, implementing caching, or profiling .NET applications
---

# Performance Optimization Skill

## When to Use This Skill

Use this skill when you need to:
- Identify and resolve performance bottlenecks
- Implement caching strategies and optimization techniques
- Profile and analyze application performance
- Optimize database queries and data access
- Improve memory management and resource utilization
- Scale applications for high-performance scenarios

## Prerequisites

- .NET 6+ application with performance concerns
- Understanding of profiling tools and techniques
- Familiarity with caching and optimization concepts
- Knowledge of database performance optimization
- Experience with memory management and resource allocation

## Step-by-Step Workflows

### 1. Performance Profiling

**Input**: Application with performance issues  
**Output: Detailed performance analysis and recommendations

```
Profile application performance with these steps:

1. Set up profiling tools and monitoring
2. Identify CPU, memory, and I/O bottlenecks
3. Analyze hot paths and critical sections
4. Measure response times and throughput
5. Identify memory leaks and allocation patterns
6. Generate performance reports and metrics
7. Create optimization plan based on findings
```

### 2. Caching Implementation

**Input**: Data access patterns and performance requirements  
**Output: Comprehensive caching strategy

```
Implement caching with these requirements:

1. Choose appropriate caching strategy (in-memory, distributed)
2. Implement cache keys and expiration policies
3. Add cache invalidation and refresh mechanisms
4. Optimize cache hit ratios and performance
5. Monitor cache effectiveness and usage
6. Handle cache fallback and error scenarios
7. Test caching under load conditions
```

### 3. Database Optimization

**Input**: Database queries and data access performance issues  
**Output: Optimized database access patterns

```
Optimize database performance with these steps:

1. Analyze slow queries and execution plans
2. Implement proper indexing strategies
3. Optimize LINQ queries and Entity Framework usage
4. Add query batching and connection pooling
5. Implement read replicas and sharding if needed
6. Monitor database performance metrics
7. Test optimizations under realistic load
```

## Performance Profiling Tools

### Built-in .NET Profiling

```csharp
// Performance Counter Implementation
public class PerformanceCounter
{
    private readonly ILogger<PerformanceCounter> _logger;
    private readonly ConcurrentDictionary<string, long> _counters = new();
    private readonly ConcurrentDictionary<string, TimeSpan> _timers = new();
    
    public PerformanceCounter(ILogger<PerformanceCounter> logger)
    {
        _logger = logger;
    }
    
    public IDisposable Measure(string operationName)
    {
        return new OperationTimer(this, operationName);
    }
    
    public void Increment(string counterName)
    {
        _counters.AddOrUpdate(counterName, 1, (key, value) => value + 1);
    }
    
    public void AddTime(string timerName, TimeSpan duration)
    {
        _timers.AddOrUpdate(timerName, duration, (key, value) => value + duration);
    }
    
    public PerformanceReport GetReport()
    {
        return new PerformanceReport
        {
            Counters = _counters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Timers = _timers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Timestamp = DateTime.UtcNow
        };
    }
    
    private class OperationTimer : IDisposable
    {
        private readonly PerformanceCounter _counter;
        private readonly string _operationName;
        private readonly Stopwatch _stopwatch;
        
        public OperationTimer(PerformanceCounter counter, string operationName)
        {
            _counter = counter;
            _operationName = operationName;
            _stopwatch = Stopwatch.StartNew();
        }
        
        public void Dispose()
        {
            _stopwatch.Stop();
            _counter.AddTime(_operationName, _stopwatch.Elapsed);
            _counter.Increment(_operationName);
        }
    }
}

public class PerformanceReport
{
    public Dictionary<string, long> Counters { get; set; }
    public Dictionary<string, TimeSpan> Timers { get; set; }
    public DateTime Timestamp { get; set; }
    
    public void LogReport(ILogger logger)
    {
        logger.LogInformation("Performance Report at {Timestamp}", Timestamp);
        
        foreach (var counter in Counters)
        {
            logger.LogInformation("Counter {Name}: {Value}", counter.Key, counter.Value);
        }
        
        foreach (var timer in Timers)
        {
            logger.LogInformation("Timer {Name}: {Duration}ms", timer.Key, timer.Value.TotalMilliseconds);
        }
    }
}
```

### Memory Profiling

```csharp
// Memory Usage Monitor
public class MemoryMonitor
{
    private readonly ILogger<MemoryMonitor> _logger;
    private readonly Timer _monitoringTimer;
    
    public MemoryMonitor(ILogger<MemoryMonitor> logger)
    {
        _logger = logger;
        _monitoringTimer = new Timer(CheckMemory, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }
    
    private void CheckMemory(object? state)
    {
        var memoryInfo = GC.GetMemoryInfo();
        var totalMemory = GC.GetTotalMemory(false);
        var gen0Collections = GC.CollectionCount(0);
        var gen1Collections = GC.CollectionCount(1);
        var gen2Collections = GC.CollectionCount(2);
        
        _logger.LogInformation(
            "Memory Usage - Total: {TotalMemory}MB, Gen0: {Gen0}, Gen1: {Gen1}, Gen2: {Gen2}",
            totalMemory / 1024 / 1024,
            gen0Collections,
            gen1Collections,
            gen2Collections);
        
        // Alert if memory usage is high
        if (totalMemory > 500 * 1024 * 1024) // 500MB
        {
            _logger.LogWarning("High memory usage detected: {TotalMemory}MB", totalMemory / 1024 / 1024);
        }
    }
    
    public void Dispose()
    {
        _monitoringTimer?.Dispose();
    }
}
```

### Request Performance Middleware

```csharp
public class RequestPerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestPerformanceMiddleware> _logger;
    private readonly PerformanceCounter _performanceCounter;
    
    public RequestPerformanceMiddleware(
        RequestDelegate next,
        ILogger<RequestPerformanceMiddleware> logger,
        PerformanceCounter performanceCounter)
    {
        _next = next;
        _logger = logger;
        _performanceCounter = performanceCounter;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        var operationName = $"{requestMethod} {requestPath}";
        
        using var timer = _performanceCounter.Measure(operationName);
        
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Request failed: {OperationName}", operationName);
            throw;
        }
        
        var statusCode = context.Response.StatusCode;
        _logger.LogInformation(
            "Request completed: {OperationName} - Status: {StatusCode} - Duration: {Duration}ms",
            operationName,
            statusCode,
            timer.Elapsed.TotalMilliseconds);
    }
}
```

## Caching Strategies

### Multi-Level Caching

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
}

public class MultiLevelCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<MultiLevelCacheService> _logger;
    private readonly PerformanceCounter _performanceCounter;
    
    public MultiLevelCacheService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        ILogger<MultiLevelCacheService> logger,
        PerformanceCounter performanceCounter)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
        _performanceCounter = performanceCounter;
    }
    
    public async Task<T?> GetAsync<T>(string key)
    {
        using var timer = _performanceCounter.Measure("Cache.GetAsync");
        
        // Try memory cache first (L1)
        if (_memoryCache.TryGetValue(key, out T memoryValue))
        {
            _performanceCounter.Increment("Cache.Memory.Hit");
            _logger.LogDebug("Cache hit (L1) for key: {Key}", key);
            return memoryValue;
        }
        
        _performanceCounter.Increment("Cache.Memory.Miss");
        
        // Try distributed cache (L2)
        try
        {
            var distributedValue = await _distributedCache.GetStringAsync(key);
            if (distributedValue != null)
            {
                var deserializedValue = JsonSerializer.Deserialize<T>(distributedValue);
                
                // Store in memory cache for faster access
                _memoryCache.Set(key, deserializedValue, TimeSpan.FromMinutes(5));
                
                _performanceCounter.Increment("Cache.Distributed.Hit");
                _logger.LogDebug("Cache hit (L2) for key: {Key}", key);
                return deserializedValue;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accessing distributed cache for key: {Key}", key);
        }
        
        _performanceCounter.Increment("Cache.Distributed.Miss");
        _logger.LogDebug("Cache miss for key: {Key}", key);
        return default(T);
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        using var timer = _performanceCounter.Measure("Cache.SetAsync");
        
        var expirationTime = expiration ?? TimeSpan.FromHours(1);
        
        // Set in memory cache
        _memoryCache.Set(key, value, expirationTime);
        
        // Set in distributed cache
        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expirationTime
            };
            
            await _distributedCache.SetStringAsync(key, serializedValue, options);
            _logger.LogDebug("Cache set for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting distributed cache for key: {Key}", key);
        }
    }
    
    public async Task RemoveAsync(string key)
    {
        using var timer = _performanceCounter.Measure("Cache.RemoveAsync");
        
        _memoryCache.Remove(key);
        
        try
        {
            await _distributedCache.RemoveAsync(key);
            _logger.LogDebug("Cache removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from distributed cache for key: {Key}", key);
        }
    }
    
    public async Task RemoveByPatternAsync(string pattern)
    {
        using var timer = _performanceCounter.Measure("Cache.RemoveByPattern");
        
        // This would require a distributed cache that supports pattern matching
        // For now, we'll implement a simple version for memory cache
        
        var keysToRemove = new List<string>();
        
        // This is a simplified implementation
        // In production, you'd use a more sophisticated approach
        foreach (var entry in _memoryCache)
        {
            if (entry.Key.Contains(pattern))
            {
                keysToRemove.Add(entry.Key);
            }
        }
        
        foreach (var key in keysToRemove)
        {
            await RemoveAsync(key);
        }
        
        _logger.LogDebug("Removed {Count} cache entries matching pattern: {Pattern}", 
            keysToRemove.Count, pattern);
    }
}
```

### Cache-Aside Pattern

```csharp
public class CacheAsideService<T>
{
    private readonly ICacheService _cacheService;
    private readonly Func<string, Task<T>> _dataLoader;
    private readonly ILogger<CacheAsideService<T>> _logger;
    private readonly TimeSpan _defaultExpiration;
    
    public CacheAsideService(
        ICacheService cacheService,
        Func<string, Task<T>> dataLoader,
        ILogger<CacheAsideService<T>> logger,
        TimeSpan? defaultExpiration = null)
    {
        _cacheService = cacheService;
        _dataLoader = dataLoader;
        _logger = logger;
        _defaultExpiration = defaultExpiration ?? TimeSpan.FromHours(1);
    }
    
    public async Task<T> GetOrSetAsync(string key, TimeSpan? expiration = null)
    {
        var cachedValue = await _cacheService.GetAsync<T>(key);
        if (cachedValue != null)
        {
            return cachedValue;
        }
        
        try
        {
            var value = await _dataLoader(key);
            if (value != null)
            {
                await _cacheService.SetAsync(key, value, expiration ?? _defaultExpiration);
            }
            
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading data for key: {Key}", key);
            throw;
        }
    }
    
    public async Task InvalidateAsync(string key)
    {
        await _cacheService.RemoveAsync(key);
        _logger.LogDebug("Cache invalidated for key: {Key}", key);
    }
}
```

## Database Optimization

### Optimized Repository Pattern

```csharp
public class OptimizedUserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly PerformanceCounter _performanceCounter;
    private readonly ILogger<OptimizedUserRepository> _logger;
    
    public OptimizedUserRepository(
        ApplicationDbContext context,
        ICacheService cacheService,
        PerformanceCounter performanceCounter,
        ILogger<OptimizedUserRepository> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _performanceCounter = performanceCounter;
        _logger = logger;
    }
    
    public async Task<User?> GetByIdAsync(int id)
    {
        using var timer = _performanceCounter.Measure("UserRepository.GetByIdAsync");
        
        var cacheKey = $"user_{id}";
        var cachedUser = await _cacheService.GetAsync<User>(cacheKey);
        
        if (cachedUser != null)
        {
            _performanceCounter.Increment("UserRepository.Cache.Hit");
            return cachedUser;
        }
        
        _performanceCounter.Increment("UserRepository.Cache.Miss");
        
        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .FirstOrDefaultAsync();
        
        if (user != null)
        {
            await _cacheService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(30));
        }
        
        return user;
    }
    
    public async Task<IEnumerable<User>> GetActiveUsersAsync(int page = 1, int pageSize = 20)
    {
        using var timer = _performanceCounter.Measure("UserRepository.GetActiveUsersAsync");
        
        var cacheKey = $"active_users_page_{page}_size_{pageSize}";
        var cachedUsers = await _cacheService.GetAsync<IEnumerable<User>>(cacheKey);
        
        if (cachedUsers != null)
        {
            _performanceCounter.Increment("UserRepository.Cache.Hit");
            return cachedUsers;
        }
        
        _performanceCounter.Increment("UserRepository.Cache.Miss");
        
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        await _cacheService.SetAsync(cacheKey, users, TimeSpan.FromMinutes(10));
        
        return users;
    }
    
    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int page = 1, int pageSize = 20)
    {
        using var timer = _performanceCounter.Measure("UserRepository.SearchUsersAsync");
        
        var cacheKey = $"search_users_{searchTerm}_page_{page}_size_{pageSize}";
        var cachedUsers = await _cacheService.GetAsync<IEnumerable<User>>(cacheKey);
        
        if (cachedUsers != null)
        {
            _performanceCounter.Increment("UserRepository.Cache.Hit");
            return cachedUsers;
        }
        
        _performanceCounter.Increment("UserRepository.Cache.Miss");
        
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => u.IsActive && 
                       (u.Name.Contains(searchTerm) || 
                        u.Email.Contains(searchTerm)))
            .OrderBy(u => u.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        await _cacheService.SetAsync(cacheKey, users, TimeSpan.FromMinutes(5));
        
        return users;
    }
    
    public async Task<User> AddAsync(User user)
    {
        using var timer = _performanceCounter.Measure("UserRepository.AddAsync");
        
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        
        // Invalidate relevant caches
        await _cacheService.RemoveByPatternAsync("active_users_*");
        await _cacheService.RemoveByPatternAsync("search_users_*");
        
        _logger.LogInformation("User created: {UserId}", user.Id);
        return user;
    }
    
    public async Task<User> UpdateAsync(User user)
    {
        using var timer = _performanceCounter.Measure("UserRepository.UpdateAsync");
        
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        
        // Invalidate specific cache entry
        await _cacheService.RemoveAsync($"user_{user.Id}");
        
        // Invalidate list caches
        await _cacheService.RemoveByPatternAsync("active_users_*");
        await _cacheService.RemoveByPatternAsync("search_users_*");
        
        _logger.LogInformation("User updated: {UserId}", user.Id);
        return user;
    }
    
    public async Task DeleteAsync(User user)
    {
        using var timer = _performanceCounter.Measure("UserRepository.DeleteAsync");
        
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        
        // Invalidate all relevant caches
        await _cacheService.RemoveAsync($"user_{user.Id}");
        await _cacheService.RemoveByPatternAsync("active_users_*");
        await _cacheService.RemoveByPatternAsync("search_users_*");
        
        _logger.LogInformation("User deleted: {UserId}", user.Id);
    }
}
```

### Query Optimization

```csharp
public class QueryOptimizer
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<QueryOptimizer> _logger;
    
    public QueryOptimizer(ApplicationDbContext context, ILogger<QueryOptimizer> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    // Optimized query with proper indexing and projection
    public async Task<IEnumerable<UserDto>> GetOptimizedUsersAsync()
    {
        return await _context.Users
            .AsNoTracking() // No tracking for read-only queries
            .Where(u => u.IsActive) // Filter early
            .Select(u => new UserDto // Project only needed columns
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                CreatedAt = u.CreatedAt
            })
            .OrderBy(u => u.Name) // Order after projection
            .ToListAsync();
    }
    
    // Batch processing for large datasets
    public async Task ProcessLargeDatasetAsync(Func<User, Task> processor, int batchSize = 100)
    {
        var maxId = await _context.Users.MaxAsync(u => (int?)u.Id) ?? 0;
        
        for (int offset = 0; offset <= maxId; offset += batchSize)
        {
            var users = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id > offset && u.Id <= offset + batchSize)
                .ToListAsync();
            
            foreach (var user in users)
            {
                await processor(user);
            }
            
            // Allow other operations to proceed
            await Task.Delay(10);
        }
    }
    
    // Compiled query for frequently used queries
    private static readonly Func<ApplicationDbContext, int, Task<User?>> GetUserByIdCompiled =
        EF.CompileAsyncQuery((ApplicationDbContext context, int id) =>
            context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id));
    
    public async Task<User?> GetUserByIdCompiledAsync(int id)
    {
        return await GetUserByIdCompiled(_context, id);
    }
    
    // Query with proper pagination
    public async Task<PagedResult<User>> GetPagedUsersAsync(int page, int pageSize)
    {
        var query = _context.Users.AsNoTracking();
        
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(u => u.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return new PagedResult<User>(items, totalCount, page, pageSize);
    }
}
```

## Memory Optimization

### Object Pooling

```csharp
public class ObjectPool<T> where T : class, new()
{
    private readonly ConcurrentBag<T> _objects = new();
    private readonly Func<T> _objectGenerator;
    private readonly Action<T>? _resetAction;
    
    public ObjectPool(Func<T>? objectGenerator = null, Action<T>? resetAction = null)
    {
        _objectGenerator = objectGenerator ?? (() => new T());
        _resetAction = resetAction;
    }
    
    public T Get()
    {
        if (_objects.TryTake(out T item))
        {
            return item;
        }
        
        return _objectGenerator();
    }
    
    public void Return(T item)
    {
        _resetAction?.Invoke(item);
        _objects.Add(item);
    }
}

// Usage for StringBuilder pooling
public class StringBuilderPool
{
    private static readonly ObjectPool<StringBuilder> _pool = new(
        () => new StringBuilder(256),
        sb => sb.Clear()
    );
    
    public static StringBuilder Get() => _pool.Get();
    
    public static void Return(StringBuilder sb) => _pool.Return(sb);
}

// Usage example
public string ProcessLargeString(IEnumerable<string> items)
{
    var sb = StringBuilderPool.Get();
    
    try
    {
        foreach (var item in items)
        {
            sb.AppendLine(item);
        }
        
        return sb.ToString();
    }
    finally
    {
        StringBuilderPool.Return(sb);
    }
}
```

### Memory-Efficient Collections

```csharp
// Memory-efficient stream processing
public class StreamProcessor
{
    public async IAsyncEnumerable<T> ProcessStreamAsync<T>(
        IEnumerable<T> source,
        Func<T, Task<T>> processor,
        int bufferSize = 100)
    {
        var buffer = new List<T>(bufferSize);
        
        foreach (var item in source)
        {
            buffer.Add(item);
            
            if (buffer.Count >= bufferSize)
            {
                foreach (var bufferedItem in buffer)
                {
                    yield return await processor(bufferedItem);
                }
                
                buffer.Clear();
            }
        }
        
        // Process remaining items
        foreach (var bufferedItem in buffer)
        {
            yield return await processor(bufferedItem);
        }
    }
}

// Memory-efficient file processing
public async Task ProcessLargeFileAsync(string filePath, Func<string, Task> lineProcessor)
{
    const int bufferSize = 8192;
    
    using var reader = new StreamReader(filePath, Encoding.UTF8, true, bufferSize);
    
    while (!reader.EndOfStream)
    {
        var line = await reader.ReadLineAsync();
        if (line != null)
        {
            await lineProcessor(line);
        }
    }
}
```

## Async Optimization

### Async/Await Best Practices

```csharp
public class AsyncOptimizationService
{
    private readonly ILogger<AsyncOptimizationService> _logger;
    
    public AsyncOptimizationService(ILogger<AsyncOptimizationService> logger)
    {
        _logger = logger;
    }
    
    // ConfigureAwait for library code
    public async Task<string> GetDataFromServiceAsync()
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync("https://api.example.com/data")
            .ConfigureAwait(false); // Don't capture context
        
        return await response.Content.ReadAsStringAsync()
            .ConfigureAwait(false);
    }
    
    // Parallel processing of multiple operations
    public async Task<IEnumerable<Result>> ProcessMultipleItemsAsync(IEnumerable<Item> items)
    {
        var tasks = items.Select(ProcessItemAsync);
        var results = await Task.WhenAll(tasks);
        return results;
    }
    
    // Batching for large number of operations
    public async Task ProcessBatchedItemsAsync(IEnumerable<Item> items, int batchSize = 10)
    {
        var batches = items.Chunk(batchSize);
        
        foreach (var batch in batches)
        {
            var batchTasks = batch.Select(ProcessItemAsync);
            await Task.WhenAll(batchTasks);
            
            // Small delay between batches
            await Task.Delay(100);
        }
    }
    
    // Timeout handling
    public async Task<T> WithTimeoutAsync<T>(Task<T> task, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        
        try
        {
            return await task.WaitAsync(cts.Token);
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds} seconds");
        }
    }
    
    // Circuit breaker pattern
    public async Task<T> CircuitBreakerAsync<T>(Func<Task<T>> operation, int maxFailures = 3, TimeSpan resetTimeout = TimeSpan.FromMinutes(1))
    {
        var failureCount = 0;
        var lastFailureTime = DateTime.MinValue;
        
        while (true)
        {
            if (failureCount >= maxFailures && DateTime.UtcNow - lastFailureTime < resetTimeout)
            {
                throw new CircuitBreakerOpenException("Circuit breaker is open");
            }
            
            try
            {
                var result = await operation();
                failureCount = 0; // Reset on success
                return result;
            }
            catch (Exception ex)
            {
                failureCount++;
                lastFailureTime = DateTime.UtcNow;
                
                if (failureCount >= maxFailures)
                {
                    _logger.LogWarning(ex, "Circuit breaker opened after {FailureCount} failures", failureCount);
                }
                
                throw;
            }
        }
    }
    
    private async Task<Item> ProcessItemAsync(Item item)
    {
        // Simulate async processing
        await Task.Delay(100);
        return item;
    }
}

public class CircuitBreakerOpenException : Exception
{
    public CircuitBreakerOpenException(string message) : base(message) { }
}
```

## Performance Monitoring

### Health Checks

```csharp
public class PerformanceHealthCheck : IHealthCheck
{
    private readonly PerformanceCounter _performanceCounter;
    private readonly ILogger<PerformanceHealthCheck> _logger;
    
    public PerformanceHealthCheck(
        PerformanceCounter performanceCounter,
        ILogger<PerformanceHealthCheck> logger)
    {
        _performanceCounter = performanceCounter;
        _logger = logger;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var report = _performanceCounter.GetReport();
            
            // Check if response times are acceptable
            var slowOperations = report.Timers
                .Where(t => t.Value.TotalMilliseconds > 1000)
                .ToList();
            
            if (slowOperations.Any())
            {
                var message = $"Slow operations detected: {string.Join(", ", slowOperations.Select(t => t.Key))}";
                _logger.LogWarning(message);
                
                return HealthCheckResult.Degraded(message);
            }
            
            // Check memory usage
            var totalMemory = GC.GetTotalMemory(false);
            if (totalMemory > 1024 * 1024 * 1024) // 1GB
            {
                var message = $"High memory usage: {totalMemory / 1024 / 1024}MB";
                _logger.LogWarning(message);
                
                return HealthCheckResult.Degraded(message);
            }
            
            return HealthCheckResult.Healthy("Performance metrics are within acceptable ranges");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Performance health check failed");
            return HealthCheckResult.Unhealthy("Performance health check failed");
        }
    }
}
```

### Metrics Collection

```csharp
public class MetricsCollector
{
    private readonly ILogger<MetricsCollector> _logger;
    private readonly ConcurrentDictionary<string, List<double>> _metrics = new();
    
    public MetricsCollector(ILogger<MetricsCollector> logger)
    {
        _logger = logger;
    }
    
    public void RecordMetric(string name, double value)
    {
        _metrics.AddOrUpdate(name, new List<double> { value }, 
            (key, list) =>
            {
                list.Add(value);
                // Keep only last 1000 values
                if (list.Count > 1000)
                {
                    return list.Skip(list.Count - 1000).ToList();
                }
                return list;
            });
    }
    
    public MetricSummary GetSummary(string metricName)
    {
        if (!_metrics.TryGetValue(metricName, out var values) || !values.Any())
        {
            return new MetricSummary(metricName, 0, 0, 0, 0);
        }
        
        var sortedValues = values.OrderBy(x => x).ToList();
        var count = sortedValues.Count;
        var mean = sortedValues.Average();
        var median = count % 2 == 0 
            ? (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2 
            : sortedValues[count / 2];
        var p95 = sortedValues[(int)(count * 0.95)];
        
        return new MetricSummary(metricName, count, mean, median, p95);
    }
    
    public void LogAllMetrics()
    {
        foreach (var metric in _metrics)
        {
            var summary = GetSummary(metric.Key);
            _logger.LogInformation(
                "Metric {Name}: Count={Count}, Mean={Mean:F2}, Median={Median:F2}, P95={P95:F2}",
                summary.Name, summary.Count, summary.Mean, summary.Median, summary.P95);
        }
    }
}

public class MetricSummary
{
    public string Name { get; }
    public int Count { get; }
    public double Mean { get; }
    public double Median { get; }
    public double P95 { get; }
    
    public MetricSummary(string name, int count, double mean, double median, double p95)
    {
        Name = name;
        Count = count;
        Mean = mean;
        Median = median;
        P95 = p95;
    }
}
```

## Best Practices

### Do's
- ✅ Profile before optimizing
- ✅ Use appropriate caching strategies
- ✅ Optimize database queries
- ✅ Implement proper memory management
- ✅ Use async/await correctly
- ✅ Monitor performance continuously
- ✅ Use connection pooling
- ✅ Implement proper error handling
- ✅ Test under realistic load
- ✅ Document performance decisions

### Don'ts
- ❌ Optimize without profiling
- ❌ Cache everything indiscriminately
- ❌ Ignore database performance
- ❌ Create memory leaks
- ❌ Block on async operations
- ❌ Skip performance monitoring
- ❌ Forget connection pooling
- ❌ Ignore error handling
- ❌ Skip load testing
- ❌ Make undocumented performance changes

## References

- [ASP.NET Core Performance Best Practices](https://docs.microsoft.com/en-us/aspnet/core/performance/)
- [Entity Framework Core Performance](https://docs.microsoft.com/en-us/ef/core/performance/)
- [C# Performance Optimization](https://docs.microsoft.com/en-us/dotnet/csharp/write-efficient-code)
- [Memory Management in .NET](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/)

---

**This skill provides comprehensive guidance for optimizing .NET application performance. Use it to create fast, efficient, and scalable applications.**
