using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критический сбой выполнения запроса к {Path}", context.Request.Path);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var details = new ProblemDetails
                {
                    Status = context.Response.StatusCode,
                    Title = "Ошибка сервера",
                    Detail = exception.Message
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(details));
            }
            else
            {
                context.Response.Redirect("/Article/Index");
            }
        }
    }
}
