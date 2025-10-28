using Microsoft.AspNetCore.Mvc.Controllers;
using Serilog;
using System.Reflection;
using System.Text;
using static AuthService.Api.Attributes.EndpointAttributes;

namespace AuthService.API.Middlewares
{
    public class RequestResponseLoggingMiddleware
    {

        private readonly RequestDelegate _next;

        public RequestResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var skip = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>()?
                .MethodInfo.GetCustomAttribute<SkipLoggingAttribute>() != null;

            if (skip)
            {
                await _next(context);
                return;
            }

            context.Request.EnableBuffering();

            // Read body request
            string requestBody = "";
            if (context.Request.ContentLength > 0 &&
                context.Request.ContentType != null &&
                context.Request.ContentType.Contains("application/json"))
            {
                context.Request.Body.Position = 0;
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                }
            }

            Log.Information("➡️ Incoming Request {Method} {Path} | TraceId: {TraceId} | Body: {Body}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier,
                string.IsNullOrWhiteSpace(requestBody) ? "(empty)" : requestBody);

            // Capture the response
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            Log.Information("⬅️ Outgoing Response {StatusCode} | TraceId: {TraceId} | Body: {Body}",
                context.Response.StatusCode,
                context.TraceIdentifier,
                string.IsNullOrWhiteSpace(responseText) ? "(empty)" : responseText);

            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}
