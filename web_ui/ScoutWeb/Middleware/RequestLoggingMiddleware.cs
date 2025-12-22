using System.Diagnostics;

namespace ScoutWeb.Middleware
{
    /// <summary>
    /// Cross-Cutting Concern: Request Logging Middleware
    /// 6. Katman: Tüm HTTP isteklerini loglar (SOA için gerekli)
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestPath = context.Request.Path;
            var requestMethod = context.Request.Method;

            _logger.LogInformation($"[SOA Request] {requestMethod} {requestPath} started");

            try
            {
                await _next(context);
                stopwatch.Stop();

                _logger.LogInformation(
                    $"[SOA Response] {requestMethod} {requestPath} completed in {stopwatch.ElapsedMilliseconds}ms - Status: {context.Response.StatusCode}"
                );
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    $"[SOA Error] {requestMethod} {requestPath} failed after {stopwatch.ElapsedMilliseconds}ms - Error: {ex.Message}"
                );
                throw;
            }
        }
    }
}
