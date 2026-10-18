using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
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
