using Blog.BLL.DTOs;
using Blog.BLL.Security;
using Blog.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    /// <summary>
    /// API-контроллер для управления комментариями к статьям блога.
    /// Наследуется от <see cref="BaseApiController"/>, который предоставляет
    /// общие механизмы маппинга результатов бизнес-логики в HTTP-ответы
    /// и проверки ролей текущего пользователя.
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

        /// <summary>
        /// Возвращает список всех комментариев.
        /// Доступно без авторизации — комментарии видны всем посетителям блога.
        /// </summary>
        /// <returns>Список комментариев.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
            => Ok(await _commentService.GetAllAsync());

        /// <summary>
        /// Возвращает комментарий по его идентификатору.
        /// Доступно без авторизации. Если комментарий не найден — возвращается 404
        /// с поясняющим сообщением в теле ответа.
        /// </summary>
        /// <param name="id">Идентификатор комментария.</param>
        /// <returns>Комментарий или 404, если не найден.</returns>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            return comment == null
                ? NotFound(new { message = "Комментарий не найден." })
                : Ok(comment);
        }

        /// <summary>
        /// Создает новый комментарий.
        /// Требует права <see cref="AppPermissions.CommentsCreate"/>.
        /// Автор комментария определяется по текущей сессии, а не по данным из тела запроса,
        /// что исключает подмену авторства.
        /// При успехе возвращается 201 Created со ссылкой на созданный ресурс.
        /// </summary>
        /// <param name="dto">Данные для создания комментария.</param>
        /// <returns>Созданный комментарий с кодом 201.</returns>
        [HttpPost]
        [Authorize(Policy = AppPermissions.CommentsCreate)]
        public async Task<IActionResult> Create([FromBody] CreateCommentDto dto)
        {
            var result = await _commentService.CreateAsync(dto, CurrentUserId);
            return ToActionResult(result,
                comment => CreatedAtAction(nameof(GetById), new { id = comment.Id }, comment));
        }

        /// <summary>
        /// Обновляет комментарий.
        /// Требует права <see cref="AppPermissions.CommentsUpdate"/>.
        /// Доступно автору, модератору или администратору: флаг <see cref="BaseApiController.IsElevatedUser"/>
        /// сообщает сервису, что пользователь имеет повышенные права, поэтому ему разрешено
        /// редактировать чужие комментарии (проверка выполняется в BLL).
        /// </summary>
        /// <param name="id">Идентификатор комментария.</param>
        /// <param name="dto">Данные для обновления.</param>
        /// <returns>Результат операции.</returns>
        [HttpPut("{id:int}")]
        [Authorize(Policy = AppPermissions.CommentsUpdate)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCommentDto dto)
        {
            var result = await _commentService.UpdateAsync(id, dto, CurrentUserId, IsElevatedUser);
            return ToActionResult(result);
        }

        /// <summary>
        /// Удаляет комментарий.
        /// Требует права <see cref="AppPermissions.CommentsDelete"/>.
        /// Как и при редактировании, <see cref="BaseApiController.IsElevatedUser"/>
        /// позволяет модератору и администратору удалять чужие комментарии.
        /// </summary>
        /// <param name="id">Идентификатор комментария.</param>
        /// <returns>Результат операции.</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = AppPermissions.CommentsDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _commentService.DeleteAsync(id, CurrentUserId, IsElevatedUser);
            return ToActionResult(result);
        }
    }
}
