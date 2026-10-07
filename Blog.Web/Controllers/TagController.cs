using Blog.BLL.DTOs;
using Blog.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    /// <summary>
    /// Контроллер меток (тегов) публикаций
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }

        /// <summary>
        /// Создание нового тега
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(TagDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateTagDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (succeeded, error, tag) = await _tagService.CreateAsync(dto);
            if (!succeeded)
                return Conflict(new { message = error });

            return CreatedAtAction(nameof(GetById), new { id = tag!.Id }, tag);
        }

        /// <summary>
        /// Получение всех существующих тегов
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TagDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var tags = await _tagService.GetAllAsync();
            return Ok(tags);
        }

        /// <summary>
        /// Получение тега по его идентификатору
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var tag = await _tagService.GetByIdAsync(id);
            if (tag == null)
                return NotFound(new { message = $"Тег с ID={id} не найден." });

            return Ok(tag);
        }

        /// <summary>
        /// Редактирование имени тега
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTagDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (succeeded, error) = await _tagService.UpdateAsync(id, dto);
            if (!succeeded)
            {
                if (error!.Contains("не найден", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { message = error });

                return Conflict(new { message = error });
            }

            return NoContent();
        }

        /// <summary>
        /// Удаление тега по идентификатору
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _tagService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Тег с ID={id} не найден." });

            return NoContent();
        }
    }
}
