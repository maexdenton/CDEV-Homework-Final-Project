using Blog.BLL.DTOs;
using Blog.BLL.Security;
using Blog.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    // API-контроллер для управления тегами публикаций.
    // Наследуется от <see cref="BaseApiController"/>, который предоставляет 
    // общие механизмы маппинга результатов бизнес-логики в HTTP-ответы и проверки ролей текущего пользователя.
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TagController : BaseApiController
    {
        private readonly ITagService _tagService;

        // Бизнес-логика - в сервисном слое.
        // Контроллер только принимает HTTP-запрос,
        // извлекает контекст пользователя и формирует ответ.
        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }

        // Получение списка всех тегов.
        // Доступно без авторизации - теги являются публичным контентом.
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll() => Ok(await _tagService.GetAllAsync());

        // Получение конкретного тега по идентификатору.
        // Доступно без авторизации. Если тег не найден — возвращается 404 с поясняющим сообщением в теле ответа.
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var tag = await _tagService.GetByIdAsync(id);
            return tag == null ? NotFound(new { message = "Тег не найден." }) : Ok(tag);
        }

        // Создает тег от имени текущего пользователя (авторство из тела запроса не принимается).
        // Требует права <see cref="AppPermissions.TagsCreate"/>. Возвращает 201 Created.
        [HttpPost]
        [Authorize(Policy = AppPermissions.TagsCreate)]
        public async Task<IActionResult> Create([FromBody] CreateTagDto dto)
        {
            var result = await _tagService.CreateAsync(dto, CurrentUserId);
            return ToActionResult(result, tag => CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag));
        }

        // Редактирует тег.
        // Доступно автору или администратору.
        // Требует права <see cref="AppPermissions.TagsUpdate"/>. Права также проверяются в сервисе.
        [HttpPut("{id:int}")]
        [Authorize(Policy = AppPermissions.TagsUpdate)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTagDto dto)
        {
            var result = await _tagService.UpdateAsync(id, dto, CurrentUserId, IsAdmin);
            return ToActionResult(result);
        }

        // Удаляет тег, доступно автору или администратору.
        // Требует права <see cref="AppPermissions.TagsDelete"/>.
        [HttpDelete("{id:int}")]
        [Authorize(Policy = AppPermissions.TagsDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _tagService.DeleteAsync(id, CurrentUserId, IsAdmin);
            return ToActionResult(result);
        }
    }
}
