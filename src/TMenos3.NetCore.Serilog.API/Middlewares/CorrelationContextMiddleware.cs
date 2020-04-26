using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;

namespace TMenos3.NetCore.Serilog.API.Middlewares
{
    public class CorrelationContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationContextMiddleware> _logger;

        public CorrelationContextMiddleware(
            RequestDelegate next,
            ILogger<CorrelationContextMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IServiceProvider serviceProvider)
        {
            var correlationId = GetCorrelationIdFromHeaderIfPresent(context);
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                context.Response?.Headers?.Add("X-CorrelationId", correlationId);
            }

            using (_logger.BeginScope("{@CorrelationId}", correlationId))
            {
                await _next(context);
            }
        }

        private string GetCorrelationIdFromHeaderIfPresent(HttpContext context)
        {
            return context.Request?.Headers.TryGetValue("X-CorrelationId", out StringValues headerValue) == true ?
                headerValue.ToString() :
                null;
        }
    }
}
