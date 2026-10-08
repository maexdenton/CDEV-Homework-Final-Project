using Blog.BLL.DTOs;
using Blog.BLL.Security;
using Blog.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    /// <summary>
    /// Контроллер меток (тегов) публикаций
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

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll() => Ok(await _tagService.GetAllAsync());

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var tag = await _tagService.GetByIdAsync(id);
            return tag == null ? NotFound(new { message = "Тег не найден." }) : Ok(tag);
        }

        /// <summary>
        /// Создание тега с фиксацией создателя
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AppPermissions.TagsCreate)]
        public async Task<IActionResult> Create([FromBody] CreateTagDto dto)
        {
            var result = await _tagService.CreateAsync(dto, CurrentUserId);
            return ToActionResult(result, tag => CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag));
        }

        /// <summary>
        /// Редактирование тега. Доступно только автору или Администратору.
        /// Модератор прав на теги НЕ имеет
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = AppPermissions.TagsUpdate)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTagDto dto)
        {
            var result = await _tagService.UpdateAsync(id, dto, CurrentUserId, IsAdmin);
            return ToActionResult(result);
        }

        /// <summary>
        /// Удаление тега. Доступно только автору или Администратору
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = AppPermissions.TagsDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _tagService.DeleteAsync(id, CurrentUserId, IsAdmin);
            return ToActionResult(result);
        }
    }
}
