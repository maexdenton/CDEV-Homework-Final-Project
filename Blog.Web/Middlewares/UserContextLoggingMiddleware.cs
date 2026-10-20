using System.Security.Claims;
using Serilog.Context;

namespace Blog.Web.Middlewares
{
    public class UserContextLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public UserContextLoggingMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var userName = context.User.Identity?.IsAuthenticated == true
                ? context.User.Identity.Name
                : "Anonymous";

            // Извлекаем роли пользователя (Administrator, Moderator, User)
            var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var userRoles = roles.Any() ? string.Join(", ", roles) : "None";

            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var fullUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";

            // Обогащаем контекст Serilog
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
