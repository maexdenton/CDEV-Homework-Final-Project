using System.Security.Claims;
using Blog.BLL.Security;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    // Базовый класс для всех API-контроллеров.
    // Наследуется от ControllerBase (не от Controller), т.к. API не использует представления.
    // Атрибут [ApiController] включает автоматическую валидацию моделей и другие API-специфичные поведения.
    // Класс абстрактный — его нельзя создать напрямую, только унаследоваться от него.
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        // Уникальный идентификатор текущего аутентифицированного пользователя
        protected string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        
        // Проверка, состоит ли текущий пользователь в роли Администратор.
        // Используется в API-методах для разграничения доступа к операциям управления
        protected bool IsAdmin => User.IsInRole(AppRoles.Administrator);

        // Проверка, состоит ли текущий пользователь в роли Модератор
        protected bool IsModerator => User.IsInRole(AppRoles.Moderator);

        // Проверка повышенных прав: Администратор или Модератор.
        // Упрощает условия в контроллерах, где обе роли имеют одинаковые права (например, редактирование чужих комментариев)
        protected bool IsElevatedUser => IsAdmin || IsModerator;


        // Преобразует результат бизнес-логики (ServiceResult) в HTTP-ответ без тела.
        // Используется, когда успешное выполнение не требует возврата данных (например, DELETE).
        // Каждый статус из BLL маппится в соответствующий HTTP-код:
        // Success -> 204 No Content, NotFound -> 404, Forbidden -> 403,
        // Conflict -> 409, InvalidData -> 400, остальные -> 500.
        protected IActionResult ToActionResult(ServiceResult result) => result.Status switch
        {
            ResultStatus.Success => NoContent(),
            ResultStatus.NotFound => NotFound(new { message = result.Message }),
            ResultStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, new { message = result.Message }),
            ResultStatus.Conflict => Conflict(new { message = result.Message }),
            ResultStatus.InvalidData => BadRequest(new { message = result.Message }),
            _ => StatusCode(500)
        };

        // Перегрузка ToActionResult для случаев, когда при успехе нужно вернуть данные.
        // Принимает функцию onSuccess, которая вызывается только если статус — Success.
        // Это позволяет избежать дублирования switch-логики в каждом API-методе
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
