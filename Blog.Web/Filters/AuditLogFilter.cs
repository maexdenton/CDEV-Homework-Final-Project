using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Web.Filters
{
    /// <summary>
    /// Фильтр действий (action filter), который ведет аудит выполнения запросов:
    /// фиксирует начало и результат действия, ошибки валидации и отказы в авторизации.
    /// Используется для наблюдаемости и последующего анализа инцидентов безопасности.
    /// </summary>
    public class AuditLogFilter : IAsyncActionFilter
    {
        private readonly ILogger<AuditLogFilter> _logger;

        public AuditLogFilter(ILogger<AuditLogFilter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Вызывается до и после выполнения действия. Логирует начало выполнения,
        /// ошибки валидации, отказы в авторизации и итоговый результат.
        /// Исключения не перехватывает: они обрабатываются в <c>GlobalExceptionMiddleware</c>.
        /// </summary>
        /// <param name="context">Контекст выполнения действия.</param>
        /// <param name="next">Делегат, вызывающий следующее звено конвейера.</param>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controllerName = context.Controller.GetType().Name;
            var actionName = context.ActionDescriptor.RouteValues["action"];
            var httpMethod = context.HttpContext.Request.Method;
            var userName = context.HttpContext.User.Identity?.Name ?? "Anonymous";

            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation("User {UserName} started {HttpMethod} {Controller}/{Action}",
                userName, httpMethod, controllerName, actionName);

            var executedContext = await next();

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .SelectMany(e => e.Value!.Errors.Select(err => err.ErrorMessage))
                    .Where(e => !string.IsNullOrEmpty(e))
                    .ToList();

                foreach (var error in errors)
                {
                    // Ошибки валидации — это ошибка пользователя, а не сбой системы, поэтому Warning.
                    _logger.LogWarning("Validation error in {Controller}/{Action} by {UserName}: {ValidationError}",
                        controllerName, actionName, userName, error);
                }
            }

            if (executedContext.Result is ForbidResult)
            {
                _logger.LogWarning("Authorization failed for {Controller}/{Action} by {UserName}",
                    controllerName, actionName, userName);
            }

            if (executedContext.Exception != null && !executedContext.ExceptionHandled)
            {
                // Само исключение логируется в GlobalExceptionMiddleware с уровнем Error;
                // здесь фиксируется только факт неуспешного завершения действия.
                _logger.LogWarning("Action {Controller}/{Action} failed for {UserName} after {Elapsed} ms",
                    controllerName, actionName, userName, elapsedMs);
            }
            else if (context.ModelState.IsValid && executedContext.Result is not ForbidResult)
            {
                _logger.LogInformation("User {UserName} completed {HttpMethod} {Controller}/{Action} successfully in {Elapsed} ms",
                    userName, httpMethod, controllerName, actionName, elapsedMs);
            }
        }
    }
}
