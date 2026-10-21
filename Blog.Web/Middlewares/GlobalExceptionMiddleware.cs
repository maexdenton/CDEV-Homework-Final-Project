using Serilog;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Middlewares
{
    /// <summary>
    /// Глобальный middleware для перехвата необработанных исключений в конвейере ASP.NET Core.
    /// Разделяет стратегии обработки для API-запросов (возврат JSON в формате ProblemDetails)
    /// и MVC-запросов (проброс исключения дальше, чтобы <c>UseExceptionHandler</c> показал страницу ошибки).
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Перехватывает исключения, возникшие при обработке запроса.
        /// Для путей, начинающихся с <c>/api</c>, формирует JSON-ответ;
        /// для остальных запросов пробрасывает исключение дальше по конвейеру.
        /// </summary>
        /// <param name="context">Контекст текущего HTTP-запроса.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var user = context.User.Identity?.Name ?? "Anonymous";

                Log.Error(ex, "Unhandled exception for {Method} {Path} by {User}. TraceId: {TraceId}",
                    context.Request.Method, context.Request.Path, user, context.TraceIdentifier);

                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    await HandleApiExceptionAsync(context, ex);
                }
                else
                {
                    // Пробрасываем для UseExceptionHandler (страница 500).
                    throw;
                }
            }
        }

        /// <summary>
        /// Формирует JSON-ответ для API-запросов при возникновении исключения.
        /// Использует стандартный <see cref="ProblemDetails"/> (RFC 7807) — единый формат описания ошибок,
        /// который понимают многие HTTP-клиенты и инструменты (Swagger, Postman и т.д.).
        /// </summary>
        /// <param name="context">Контекст текущего HTTP-запроса.</param>
        /// <param name="exception">Перехваченное исключение.</param>
        private static async Task HandleApiExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var details = new ProblemDetails
            {
                Status = context.Response.StatusCode,
                Title = "Ошибка сервера",
                // Detail содержит текст исключения (в production этот текст,
                // возможно, стоит скрывать, чтобы не раскрывать внутренние детали).
                Detail = exception.Message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(details));
        }
    }
}
