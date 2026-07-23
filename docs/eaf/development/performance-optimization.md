

# Performance Optimization in EAF

This document provides guidelines for optimizing the performance of applications built with the Enterprise Application Framework (EAF).

## 1. Database Optimization

*   **Use Indexes**: Ensure that all frequently queried columns have indexes.
*   **Optimize Queries**: Use efficient queries and avoid N+1 query problems.
*   **Use Caching**: Cache frequently accessed data to reduce database load.
*   **Use Connection Pooling**: Use connection pooling to reduce the overhead of creating new database connections.
*   **Use Asynchronous Operations**: Use asynchronous operations to avoid blocking the main thread.

## 2. Caching

*   **Use Memory Caching**: Use memory caching for frequently accessed data that does not change often.
*   **Use Distributed Caching**: Use distributed caching for data that needs to be shared across multiple servers.
*   **Use Cache Invalidation**: Use cache invalidation to ensure that cached data is up-to-date.

## 3. Application Code

*   **Use Asynchronous Operations**: Use asynchronous operations to avoid blocking the main thread.
*   **Minimize Object Creation**: Minimize object creation to reduce memory allocation and garbage collection overhead.
*   **Use Efficient Data Structures**: Use efficient data structures to improve performance.
*   **Avoid Blocking Calls**: Avoid blocking calls to prevent the application from becoming unresponsive.

## 4. Web Application

*   **Enable Compression**: Enable compression to reduce the size of HTTP responses.
*   **Use a Content Delivery Network (CDN)**: Use a CDN to serve static assets.
*   **Optimize Images**: Optimize images to reduce their file size.
*   **Minify CSS and JavaScript**: Minify CSS and JavaScript files to reduce their file size.
*   **Use HTTP/2**: Use HTTP/2 to improve the performance of HTTP requests.

## 5. Monitoring

*   **Monitor Performance**: Monitor the performance of your application to identify bottlenecks.
*   **Use Profiling Tools**: Use profiling tools to identify performance issues in your code.
*   **Use Logging**: Use logging to track events and errors.

By following these guidelines, you can optimize the performance of your EAF applications and provide a better user experience.

## 6. Melhorias Implementadas (v2.0)

As seguintes otimizações de performance foram implementadas no EAF (PR #61):

### Backend
- **Remoção do BinaryFormatter** (Specs 01-02): Substituído por `ExtendedXmlSerializer` + `System.Text.Json` fallback nos módulos de cache
- **Correção de Fire-and-Forget** (Spec 03): Chamadas async aguardadas corretamente no cache
- **Batch Delete** (Spec 04): Operações de exclusão em lote no Audit Log Worker
- **IHttpClientFactory** (Spec 05): Eliminação de `new HttpClient()` nos auth providers
- **Response Compression** (Spec 12): Brotli + Gzip no template API
- **AsNoTracking** (Spec 13): Queries read-only otimizadas no EF Core
- **Task.CompletedTask** (Spec 14): Eliminação de alocações desnecessárias

### Multi-Database (Specs 06-08)
- Suporte a PostgreSQL e MySQL além de SQL Server
- Provider switch via configuração `Database:Provider`

### Angular (Specs 09-11)
- `takeUntilDestroyed()` para cleanup de subscriptions
- Lazy loading com budgets de bundle
- `ChangeDetectionStrategy.OnPush` em 9 componentes stateless

### SOLID Refactoring (Specs 80-86)
- Remoção do Service Locator anti-pattern
- Factory Pattern para KeyVault providers
- Interface Segregation no Worker
- SRP extraction no MiddlewareWebCoreModule

Para detalhes completos, consulte o [Changelog v2.0](./CHANGELOG-v2.md).

