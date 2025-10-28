using System.Diagnostics;

namespace AuthService.API.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Si ya viene un TraceId del request (por ejemplo, de otro microservicio)
            string traceId = context.TraceIdentifier ?? Activity.Current?.Id ?? Guid.NewGuid().ToString();

            context.Response.Headers["X-Correlation-Id"] = traceId;
            Serilog.Context.LogContext.PushProperty("TraceId", traceId);

            await _next(context);
        }
    }

    public static class CorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
