using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    /// <summary>
    /// MVC-контроллер для отображения кастомных страниц ошибок.
    /// Вызывается middleware (<c>UseExceptionHandler</c>, <c>UseStatusCodePagesWithReExecute</c>)
    /// при возникновении соответствующих HTTP-кодов, а не напрямую пользователем.
    /// Атрибут <see cref="ApiExplorerSettingsAttribute"/> с <c>IgnoreApi = true</c>
    /// исключает контроллер из документации Swagger, т.к. это MVC-страницы,
    /// а не часть REST API.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : Controller
    {
        /// <summary>
        /// Отображает общую страницу ошибки с кодом 500.
        /// </summary>
        /// <returns>Представление <c>Error</c>.</returns>
        [Route("Error")]
        public IActionResult Error()
        {
            Response.StatusCode = 500;
            return View("Error");
        }

        /// <summary>
        /// Отображает страницу, соответствующую указанному HTTP-коду.
        /// Для 403 возвращается <c>AccessDenied</c>, для 404 — <c>NotFound</c>,
        /// для остальных кодов — общее представление <c>Error</c>.
        /// </summary>
        /// <param name="statusCode">HTTP-код ошибки.</param>
        /// <returns>Представление, соответствующее коду ошибки.</returns>
        [Route("Error/{statusCode:int}")]
        public IActionResult StatusCodeError(int statusCode)
        {
            Response.StatusCode = statusCode;
            return statusCode switch
            {
                403 => View("AccessDenied"),
                404 => View("NotFound"),
                _ => View("Error")
            };
        }
    }
}
