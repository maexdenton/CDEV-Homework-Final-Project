using Blog.BLL.DTOs;
using Blog.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    /// <summary>
    /// Контроллер комментариев к статьям блога
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        /// <summary>
        /// Создание нового комментария к статье
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreateCommentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (succeeded, error, comment) = await _commentService.CreateAsync(dto);
            if (!succeeded)
                return NotFound(new { message = error });

            return CreatedAtAction(nameof(GetById), new { id = comment!.Id }, comment);
        }

        /// <summary>
        /// Получение списка всех комментариев
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CommentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var comments = await _commentService.GetAllAsync();
            return Ok(comments);
        }

        /// <summary>
        /// Получение комментария по его идентификатору
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
                return NotFound(new { message = $"Комментарий с ID={id} не найден." });

            return Ok(comment);
        }

        /// <summary>
        /// Редактирование текста существующего комментария
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCommentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _commentService.UpdateAsync(id, dto);
            if (!updated)
                return NotFound(new { message = $"Комментарий с ID={id} не найден." });

            return NoContent();
        }

        /// <summary>
        /// Удаление комментария по его идентификатору
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _commentService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Комментарий с ID={id} не найден." });

            return NoContent();
        }
    }
}
