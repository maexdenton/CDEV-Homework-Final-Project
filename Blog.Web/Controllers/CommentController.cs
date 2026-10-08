using Blog.BLL.DTOs;
using Blog.BLL.Security;
using Blog.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    /// <summary>
    /// Контроллер комментариев к статьям блога
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CommentController : BaseApiController
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll() => Ok(await _commentService.GetAllAsync());

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            return comment == null ? NotFound(new { message = "Комментарий не найден." }) : Ok(comment);
        }

        /// <summary>
        /// Создание комментария. Автор извлекается из Claims текущей сессии
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AppPermissions.CommentsCreate)]
        public async Task<IActionResult> Create([FromBody] CreateCommentDto dto)
        {
            var result = await _commentService.CreateAsync(dto, CurrentUserId);
            return ToActionResult(result, comment => CreatedAtAction(nameof(GetById), new { id = comment.Id }, comment));
        }

        /// <summary>
        /// Редактирование комментария. Доступно автору комментария, Модератору и Администратору
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = AppPermissions.CommentsUpdate)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCommentDto dto)
        {
            var result = await _commentService.UpdateAsync(id, dto, CurrentUserId, IsElevatedUser);
            return ToActionResult(result);
        }

        /// <summary>
        /// Удаление комментария. Доступно автору комментария, Модератору и Администратору
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = AppPermissions.CommentsDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _commentService.DeleteAsync(id, CurrentUserId, IsElevatedUser);
            return ToActionResult(result);
        }
    }
}
