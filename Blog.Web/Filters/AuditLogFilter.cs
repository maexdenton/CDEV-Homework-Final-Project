using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Blog.Web.Filters
{
    public class AuditLogFilter : IAsyncActionFilter
    {
        private readonly ILogger<AuditLogFilter> _logger;

        public AuditLogFilter(ILogger<AuditLogFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controllerName = context.Controller.GetType().Name;
            var actionName = context.ActionDescriptor.RouteValues["action"];
            var httpMethod = context.HttpContext.Request.Method;
            var userName = context.HttpContext.User.Identity?.Name ?? "Anonymous";

            var stopwatch = Stopwatch.StartNew();

            // 1. Логируем начало выполнения действия
            _logger.LogInformation("User {UserName} started {HttpMethod} {Controller}/{Action}",
                userName, httpMethod, controllerName, actionName);

            var executedContext = await next(); // Выполнение действия

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // 2. Проверяем ошибки валидации (ModelState)
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .SelectMany(e => e.Value!.Errors.Select(err => err.ErrorMessage))
                    .Where(e => !string.IsNullOrEmpty(e))
                    .ToList();

                foreach (var error in errors)
                {
                    // Логируем ошибки валидации с уровнем Warning
                    _logger.LogWarning("Validation error in {Controller}/{Action} by {UserName}: {ValidationError}",
                        controllerName, actionName, userName, error);
                }
            }

            // 3. Проверяем ошибки авторизации (например, возврат Forbid())
            if (executedContext.Result is Microsoft.AspNetCore.Mvc.ForbidResult)
            {
                _logger.LogWarning("Authorization failed for {Controller}/{Action} by {UserName}",
                    controllerName, actionName, userName);
            }

            // 4. Логируем результат выполнения
            if (executedContext.Exception != null && !executedContext.ExceptionHandled)
            {
                // Само исключение будет залогировано в GlobalExceptionMiddleware
                _logger.LogWarning("Action {Controller}/{Action} failed for {UserName} after {Elapsed} ms",
                    controllerName, actionName, userName, elapsedMs);
            }
            else if (context.ModelState.IsValid && !(executedContext.Result is Microsoft.AspNetCore.Mvc.ForbidResult))
            {
                // Успех (только если нет ошибок валидации и запретов)
                _logger.LogInformation("User {UserName} completed {HttpMethod} {Controller}/{Action} successfully in {Elapsed} ms",
                    userName, httpMethod, controllerName, actionName, elapsedMs);
            }
        }
    }
}
