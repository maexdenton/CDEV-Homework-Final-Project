using Serilog;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Middlewares
{
    // Глобальный middleware для перехвата необработанных исключений в конвейере ASP.NET Core.
    // Разделяет стратегии обработки для API-запросов (возврат JSON) и MVC-запросов 
    // (проброс исключения дальше, чтобы UseExceptionHandler показал страницу ошибки).
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        public GlobalExceptionMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var user = context.User.Identity?.Name ?? "Anonymous";

                // Логируем ошибку через Serilog с контекстом
                Log.Error(ex, "Unhandled exception for {Method} {Path} by {User}. TraceId: {TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    user,
                    context.TraceIdentifier);

                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    await HandleApiExceptionAsync(context, ex);
                }
                else
                {
                    throw; // Пробрасываем для UseExceptionHandler (страница 500)
                }
            }
        }

        // Формирует JSON-ответ для API-запросов при возникновении исключения.
        // Использует стандартный ProblemDetails (RFC 7807) — единый формат описания ошибок,
        // который понимают многие HTTP-клиенты и инструменты (Swagger, Postman и т.д.).
        private static async Task HandleApiExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var details = new ProblemDetails
            {
                Status = context.Response.StatusCode,
                Title = "Ошибка сервера",

                // Detail содержит текст исключения. В production этот текст,
                // возможно, стоит скрывать, чтобы не раскрывать внутренние детали
                Detail = exception.Message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(details));
        }
    }
}
