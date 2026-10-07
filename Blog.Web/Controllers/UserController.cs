using Blog.BLL.DTOs;
using Blog.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    /// <summary>
    /// Контроллер управления пользователями и регистрации.
    /// Предоставляет RESTful endpoints для CRUD-операций над пользователями
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Регистрация нового пользователя с автоматическим присвоением роли "Пользователь"
        /// </summary>
        /// <param name="dto">Данные учетной записи для регистрации</param>
        /// <response code="201">Пользователь успешно зарегистрирован</response>
        /// <response code="400">Ошибки валидации пароля или полей</response>
        /// <response code="409">Пользователь с таким email уже существует</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (succeeded, error, user) = await _userService.RegisterAsync(dto);

            if (!succeeded)
            {
                if (error!.Contains("уже зарегистрирован", StringComparison.OrdinalIgnoreCase))
                    return Conflict(new { message = error });

                return BadRequest(new { message = error });
            }

            _logger.LogInformation("Зарегистрирован новый пользователь ID={UserId}", user!.Id);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        /// <summary>
        /// Получение списка всех зарегистрированных пользователей
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        /// <summary>
        /// Получение информации о пользователе по его идентификатору
        /// </summary>
        /// <param name="id">GUID идентификатор пользователя</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = $"Пользователь с ID '{id}' не найден." });

            return Ok(user);
        }

        /// <summary>
        /// Редактирование профиля пользователя (имя, фамилия, телефон)
        /// </summary>
        /// <param name="id">Идентификатор редактируемого пользователя</param>
        /// <param name="dto">Новые данные профиля</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _userService.UpdateAsync(id, dto);
            if (!updated)
                return NotFound(new { message = $"Пользователь с ID '{id}' не найден." });

            return NoContent();
        }

        /// <summary>
        /// Удаление учетной записи пользователя
        /// </summary>
        /// <param name="id">Идентификатор удаляемого пользователя</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _userService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Пользователь с ID '{id}' не найден." });

            _logger.LogWarning("Удален пользователь ID={UserId}", id);
            return NoContent();
        }
    }
}
