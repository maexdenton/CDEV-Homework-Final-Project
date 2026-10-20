using Blog.BLL.DTOs;
using Blog.BLL.Security;
using Blog.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    /// <summary>
    /// REST API для CRUD-операций над пользователями.
    /// Наследуется от <see cref="BaseApiController"/>, который предоставляет
    /// общие механизмы маппинга результатов бизнес-логики в HTTP-ответы
    /// и проверки ролей текущего пользователя.
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
        /// Регистрирует нового пользователя с базовой ролью «Пользователь».
        /// Возвращает 201 Created, а при занятом email — 409 Conflict.
        /// </summary>
        /// <param name="dto">Данные для регистрации.</param>
        /// <returns>Созданный пользователь с кодом 201 или ошибка конфликта.</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            var result = await _userService.RegisterAsync(dto);
            return ToActionResult(result, user =>
                CreatedAtAction(nameof(GetById), new { id = user.Id }, user));
        }

        /// <summary>
        /// Возвращает список всех пользователей.
        /// Доступно только администратору (политика <see cref="AppPermissions.UsersView"/>).
        /// </summary>
        /// <returns>Список пользователей.</returns>
        [HttpGet]
        [Authorize(Policy = AppPermissions.UsersView)]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userService.GetAllAsync();
            return ToActionResult(result, Ok);
        }

        /// <summary>
        /// Возвращает профиль пользователя по идентификатору.
        /// Администратор может просматривать любого пользователя, обычный пользователь — только себя.
        /// </summary>
        /// <param name="id">Идентификатор пользователя.</param>
        /// <returns>Профиль пользователя или 403, если доступ запрещен.</returns>
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
        /// Обновляет профиль пользователя.
        /// Доступно только владельцу профиля или администратору.
        /// </summary>
        /// <param name="id">Идентификатор пользователя.</param>
        /// <param name="dto">Данные для обновления.</param>
        /// <returns>Результат операции (204 No Content при успехе).</returns>
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
        /// Удаляет учетную запись пользователя.
        /// Доступно только администратору (политика <see cref="AppPermissions.UsersDelete"/>).
        /// </summary>
        /// <param name="id">Идентификатор пользователя.</param>
        /// <returns>Результат операции (204 No Content при успехе).</returns>
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
        /// Изменяет роль пользователя.
        /// Доступно только администратору (политика <see cref="AppPermissions.RolesManage"/>).
        /// </summary>
        /// <param name="id">Идентификатор пользователя.</param>
        /// <param name="dto">Данные с новой ролью.</param>
        /// <returns>Результат операции (204 No Content при успехе).</returns>
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
