using Blog.BLL.DTOs;
using Blog.BLL.Security;
using Blog.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    // API-контроллер для управления комментариями к статьям блога.
    // Наследуется от <see cref="BaseApiController"/>, который предоставляет
    // общие механизмы маппинга результатов бизнес-логики в HTTP-ответы и проверки ролей текущего пользователя.
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CommentController : BaseApiController
    {
        private readonly ICommentService _commentService;

        // Вся бизнес-логика (проверка прав, валидация, работа с БД) инкапсулирована в сервисном слое.
        // Контроллер отвечает только за прием HTTP-запроса, извлечение контекста (ID пользователя, его права) и формирование ответа
        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // Получение списка всех комментариев.
        // Доступно без авторизации — комментарии видны всем посетителям блога.
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll() => Ok(await _commentService.GetAllAsync());

        // Получение конкретного комментария по идентификатору.
        // Доступно без авторизации. Если комментарий не найден — возвращается 404 с поясняющим сообщением в теле ответа.
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            return comment == null ? NotFound(new { message = "Комментарий не найден." }) : Ok(comment);
        }

        // Создание нового комментария.
        // Требует права <see cref="AppPermissions.CommentsCreate"/>.
        // Автор определяется по текущей сессии (<see cref="BaseApiController.CurrentUserId"/>),
        // а не по данным из тела запроса — это исключает подмену авторства.
        // При успехе возвращается 201 Created со ссылкой на только что созданный ресурс.
        [HttpPost]
        [Authorize(Policy = AppPermissions.CommentsCreate)]
        public async Task<IActionResult> Create([FromBody] CreateCommentDto dto)
        {
            var result = await _commentService.CreateAsync(dto, CurrentUserId);
            return ToActionResult(result, comment => CreatedAtAction(nameof(GetById), new { id = comment.Id }, comment));
        }

        // Редактирование комментария.
        // Требует права <see cref="AppPermissions.CommentsUpdate"/>.
        // Флаг <see cref="BaseApiController.IsElevatedUser"/> сообщает сервису, 
        // что текущий пользователь — модератор или администратор, поэтому ему разрешено редактировать чужие комментарии (проверка выполняется в BLL).
        [HttpPut("{id:int}")]
        [Authorize(Policy = AppPermissions.CommentsUpdate)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCommentDto dto)
        {
            var result = await _commentService.UpdateAsync(id, dto, CurrentUserId, IsElevatedUser);
            return ToActionResult(result);
        }

        // Удаление комментария.
        // Требует права <see cref="AppPermissions.CommentsDelete"/>.
        // Как и при редактировании, <see cref="BaseApiController.IsElevatedUser"/> 
        // позволяет модератору и администратору удалять чужие комментарии.
        [HttpDelete("{id:int}")]
        [Authorize(Policy = AppPermissions.CommentsDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _commentService.DeleteAsync(id, CurrentUserId, IsElevatedUser);
            return ToActionResult(result);
        }
    }
}
