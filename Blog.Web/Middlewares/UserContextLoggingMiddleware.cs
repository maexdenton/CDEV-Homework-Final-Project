using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Blog.Web.Middlewares
{
    /// <summary>
    /// Middleware, обогащающий контекст Serilog данными о текущем пользователе
    /// (имя, роли, IP-адрес, полный URL запроса). Это позволяет фильтровать и группировать
    /// логи по конкретному пользователю или запросу без дублирования этих полей в каждом сообщении.
    /// </summary>
    public class UserContextLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public UserContextLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Добавляет свойства пользователя в <see cref="LogContext"/> на время выполнения запроса.
        /// Свойства автоматически удаляются после завершения запроса благодаря <c>using</c>.
        /// </summary>
        /// <param name="context">Контекст текущего HTTP-запроса.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            var userName = context.User.Identity?.IsAuthenticated == true
                ? context.User.Identity.Name
                : "Anonymous";

            // Роли извлекаются из claims, чтобы логи содержали актуальный набор ролей
            // на момент выполнения запроса (без дополнительных обращений к БД).
            var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var userRoles = roles.Any() ? string.Join(", ", roles) : "None";

            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var fullUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";

            // LogContext.PushProperty добавляет свойства ко всем логам, созданным внутри блока using.
            using (LogContext.PushProperty("UserName", userName))
            using (LogContext.PushProperty("UserRoles", userRoles))
            using (LogContext.PushProperty("ClientIp", clientIp))
            using (LogContext.PushProperty("FullUrl", fullUrl))
            {
                await _next(context);
            }
        }
    }
}
