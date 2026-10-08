using System.Security.Claims;
using Blog.BLL.Security;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        protected bool IsAdmin => User.IsInRole(AppRoles.Administrator);
        protected bool IsModerator => User.IsInRole(AppRoles.Moderator);
        protected bool IsElevatedUser => IsAdmin || IsModerator;

        protected IActionResult ToActionResult(ServiceResult result) => result.Status switch
        {
            ResultStatus.Success => NoContent(),
            ResultStatus.NotFound => NotFound(new { message = result.Message }),
            ResultStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, new { message = result.Message }),
            ResultStatus.Conflict => Conflict(new { message = result.Message }),
            ResultStatus.InvalidData => BadRequest(new { message = result.Message }),
            _ => StatusCode(500)
        };

        protected IActionResult ToActionResult<T>(ServiceResult<T> result, Func<T, IActionResult> onSuccess) => result.Status switch
        {
            ResultStatus.Success => onSuccess(result.Data!),
            ResultStatus.NotFound => NotFound(new { message = result.Message }),
            ResultStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, new { message = result.Message }),
            ResultStatus.Conflict => Conflict(new { message = result.Message }),
            ResultStatus.InvalidData => BadRequest(new { message = result.Message }),
            _ => StatusCode(500)
        };
    }
}
