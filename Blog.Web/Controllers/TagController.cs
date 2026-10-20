using Blog.BLL.DTOs;
using Blog.BLL.Security;
using Blog.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    /// <summary>
    /// API-контроллер для управления тегами публикаций.
    /// Наследуется от <see cref="BaseApiController"/>, который предоставляет
    /// общие механизмы маппинга результатов бизнес-логики в HTTP-ответы
    /// и проверки ролей текущего пользователя.
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TagController : BaseApiController
    {
        private readonly ITagService _tagService;

        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }

        /// <summary>
        /// Возвращает список всех тегов.
        /// Доступно без авторизации — теги являются публичным контентом.
        /// </summary>
        /// <returns>Список тегов.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var result = await _tagService.GetAllAsync();
            return ToActionResult(result, Ok);
        }

        /// <summary>
        /// Возвращает тег по его идентификатору.
        /// Доступно без авторизации. Если тег не найден — возвращается 404
        /// с поясняющим сообщением в теле ответа.
        /// </summary>
        /// <param name="id">Идентификатор тега.</param>
        /// <returns>Тег или 404, если не найден.</returns>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _tagService.GetByIdAsync(id);
            return ToActionResult(result, Ok);
        }

        /// <summary>
        /// Создает тег от имени текущего пользователя.
        /// Авторство берется из сессии, а не из тела запроса, что исключает подмену автора.
        /// Требует права <see cref="AppPermissions.TagsCreate"/>.
        /// При успехе возвращает 201 Created со ссылкой на созданный ресурс.
        /// </summary>
        /// <param name="dto">Данные для создания тега.</param>
        /// <returns>Созданный тег с кодом 201.</returns>
        [HttpPost]
        [Authorize(Policy = AppPermissions.TagsCreate)]
        public async Task<IActionResult> Create([FromBody] CreateTagDto dto)
        {
            var result = await _tagService.CreateAsync(dto, CurrentUserId);
            return ToActionResult(result,
                tag => CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag));
        }

        /// <summary>
        /// Обновляет тег.
        /// Доступно автору или администратору. Требует права <see cref="AppPermissions.TagsUpdate"/>.
        /// Флаг <see cref="BaseApiController.IsAdmin"/> передается в сервис,
        /// где выполняется окончательная проверка прав.
        /// </summary>
        /// <param name="id">Идентификатор тега.</param>
        /// <param name="dto">Данные для обновления.</param>
        /// <returns>Результат операции.</returns>
        [HttpPut("{id:int}")]
        [Authorize(Policy = AppPermissions.TagsUpdate)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTagDto dto)
        {
            var result = await _tagService.UpdateAsync(id, dto, CurrentUserId, IsAdmin);
            return ToActionResult(result);
        }

        /// <summary>
        /// Удаляет тег.
        /// Доступно автору или администратору. Требует права <see cref="AppPermissions.TagsDelete"/>.
        /// Как и при обновлении, окончательная проверка прав выполняется в сервисе.
        /// </summary>
        /// <param name="id">Идентификатор тега.</param>
        /// <returns>Результат операции.</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = AppPermissions.TagsDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _tagService.DeleteAsync(id, CurrentUserId, IsAdmin);
            return ToActionResult(result);
        }
    }
}
