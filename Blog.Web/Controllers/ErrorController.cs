using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    // Контроллер для отображения кастомных страниц ошибок.
    // Вызывается middleware (UseExceptionHandler, UseStatusCodePagesWithReExecute) при возникновении соответствующих HTTP-кодов, а не напрямую пользователем.
    // Атрибут [ApiExplorerSettings(IgnoreApi = true)] исключает контроллер из документации Swagger, т.к. это MVC-страницы, а не часть REST API.
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : Controller
    {
        [Route("Error")]
        public IActionResult Error()
        {
            Response.StatusCode = 500;
            return View("Error");
        }

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
