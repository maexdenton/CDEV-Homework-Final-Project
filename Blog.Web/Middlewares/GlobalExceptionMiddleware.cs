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
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // Перехватывает исключения из последующего конвейера для каждого HTTP-запроса
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Логируем исключение с указанием пути запроса — это помогает при диагностике 
                _logger.LogError(ex, "Критический сбой выполнения запроса к {Path}", context.Request.Path);

                // Стратегия зависит от типа запроса:
                // - для API возвращаем структурированный JSON-ответ;
                // - для MVC пробрасываем исключение дальше, чтобы UseExceptionHandler сохранил статус-код 500 и отрисовал кастомную страницу ошибки.
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    await HandleApiExceptionAsync(context, ex);
                }
                else
                {
                    // Пробрасываем исключение для UseExceptionHandler,
                    // чтобы сохранить статус-код 500 и показать страницу ошибки.
                    throw;
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
