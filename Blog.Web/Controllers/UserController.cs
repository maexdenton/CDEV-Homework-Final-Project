using Blog.BLL.DTOs;
using Blog.BLL.Security;
using Blog.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    /// <summary>
    /// Контроллер управления пользователями и регистрации.
    /// Предоставляет RESTful endpoints для CRUD-операций над пользователями
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserController : BaseApiController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Регистрация нового пользователя. Открытый доступ.
        /// Автоматически присваивает базовую роль "Пользователь"
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            var result = await _userService.RegisterAsync(dto);
            return ToActionResult(result, user => CreatedAtAction(nameof(GetById), new { id = user.Id }, user));
        }

        /// <summary>
        /// Получение реестра всех пользователей. Доступно только Администратору
        /// </summary>
        [HttpGet]
        [Authorize(Policy = AppPermissions.UsersView)]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        /// <summary>
        /// Просмотр профиля: Администратор может просматривать любого, Пользователь — только себя
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _userService.GetByIdAsync(id, CurrentUserId, IsAdmin);
            return ToActionResult(result, Ok);
        }

        /// <summary>
        /// Редактирование профиля: только владелец или Администратор
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto dto)
        {
            var result = await _userService.UpdateAsync(id, dto, CurrentUserId, IsAdmin);
            return ToActionResult(result);
        }

        /// <summary>
        /// Удаление аккаунта: только Администратор
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = AppPermissions.UsersDelete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _userService.DeleteAsync(id, CurrentUserId, IsAdmin);
            return ToActionResult(result);
        }

        /// <summary>
        /// Управление ролями: только Администратор
        /// </summary>
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
