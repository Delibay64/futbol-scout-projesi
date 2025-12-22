using Microsoft.Extensions.Caching.Memory;

namespace ScoutWeb.Middleware
{
    /// <summary>
    /// Cross-Cutting Concern: Response Caching Middleware
    /// 6. Katman: API yanıtlarını cache'ler (performans için)
    /// </summary>
    public class ResponseCachingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ResponseCachingMiddleware> _logger;

        public ResponseCachingMiddleware(
            RequestDelegate next,
            IMemoryCache cache,
            ILogger<ResponseCachingMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Sadece GET istekleri için cache kullan
            if (context.Request.Method != "GET")
            {
                await _next(context);
                return;
            }

            var cacheKey = $"Response_{context.Request.Path}_{context.Request.QueryString}";

            // Cache'de var mı kontrol et
            if (_cache.TryGetValue(cacheKey, out string? cachedResponse))
            {
                _logger.LogInformation($"[Cache Hit] {context.Request.Path}");
                context.Response.Headers["X-Cache"] = "HIT";
                await context.Response.WriteAsync(cachedResponse ?? "");
                return;
            }

            // Cache'de yok, normal akışa devam et
            _logger.LogInformation($"[Cache Miss] {context.Request.Path}");
            context.Response.Headers["X-Cache"] = "MISS";

            await _next(context);

            // Not: Gerçek bir uygulamada response body'yi cache'lemek için
            // daha karmaşık bir yapı gerekir (response stream'i yakalama)
        }
    }
}
