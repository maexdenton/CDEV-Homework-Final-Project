using Blog.BLL.DTOs;
using Blog.BLL.Security;
using Blog.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    // REST API для CRUD-операций над пользователями.
    // Наследует <see cref="BaseApiController"/> для маппинга результатов в HTTP-ответы и проверки ролей.
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserController : BaseApiController
    {
        private readonly IUserService _userService;

        // Бизнес-логика - в сервисном слое.
        // Контроллер принимает HTTP-запрос, извлекает контекст пользователя и формирует ответ.
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // Регистрирует пользователя с базовой ролью "Пользователь".
        // Возвращает 201 Created, а при занятом email — 409 Conflict.
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            var result = await _userService.RegisterAsync(dto);
            return ToActionResult(result, user => CreatedAtAction(nameof(GetById), new { id = user.Id }, user));
        }

        // Возвращает список всех пользователей.
        // Доступно только администратору (политика <see cref="AppPermissions.UsersView"/>).
        [HttpGet]
        [Authorize(Policy = AppPermissions.UsersView)]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        // Просмотр профиля: Администратор может просматривать любого, Пользователь — только себя
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _userService.GetByIdAsync(id, CurrentUserId, IsAdmin);
            return ToActionResult(result, Ok);
        }

        // Редактирование профиля: только владелец или Администратор
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto dto)
        {
            var result = await _userService.UpdateAsync(id, dto, CurrentUserId, IsAdmin);
            return ToActionResult(result);
        }

        // Удаление аккаунта: только Администратор
        [HttpDelete("{id}")]
        [Authorize(Policy = AppPermissions.UsersDelete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _userService.DeleteAsync(id, CurrentUserId, IsAdmin);
            return ToActionResult(result);
        }

        // Управление ролями: только Администратор
        [HttpPut("{id}/role")]
        [Authorize(Policy = AppPermissions.RolesManage)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ChangeRole(string id, [FromBody] ChangeUserRoleDto dto)
        {
            var result = await _userService.ChangeUserRoleAsync(id, dto.RoleName);
            return ToActionResult(result);
        }
    }
}
