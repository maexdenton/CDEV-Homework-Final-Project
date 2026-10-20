using Blog.BLL.DTOs;
using Blog.BLL.Security;
using Blog.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    /// <summary>
    /// Контроллер для управления публикациями (статьями).
    /// Обеспечивает полный CRUD, поиск и выборку по автору.
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ArticleController : BaseApiController
    {
        private readonly IArticleService _articleService;

        public ArticleController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        /// <summary>
        /// Возвращает список статей с необязательной фильтрацией по поисковой строке и тегу.
        /// </summary>
        /// <param name="search">Поисковая строка для фильтрации (необязательно).</param>
        /// <param name="tag">Название тега для фильтрации (необязательно).</param>
        /// <returns>Список статей.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? tag)
            => Ok(await _articleService.GetAllAsync(search, tag));

        /// <summary>
        /// Возвращает статью по ее идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор статьи.</param>
        /// <returns>Статья или 404, если не найдена.</returns>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var article = await _articleService.GetByIdAsync(id);
            return article == null
                ? NotFound(new { message = "Статья не найдена." })
                : Ok(article);
        }

        /// <summary>
        /// Возвращает статьи указанного автора.
        /// </summary>
        /// <param name="authorId">Идентификатор автора.</param>
        /// <returns>Список статей автора.</returns>
        [HttpGet("author/{authorId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByAuthor(string authorId)
            => Ok(await _articleService.GetByAuthorIdAsync(authorId));

        /// <summary>
        /// Создает новую статью.
        /// </summary>
        /// <param name="dto">Данные для создания статьи.</param>
        /// <returns>Созданная статья с кодом 201.</returns>
        [HttpPost]
        [Authorize(Policy = AppPermissions.ArticlesCreate)]
        [ProducesResponseType(typeof(ArticleDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateArticleDto dto)
        {
            // Автор всегда берется из claims токена/куки, чтобы исключить подмену через тело запроса.
            var created = await _articleService.CreateAsync(dto, CurrentUserId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Обновляет статью. Доступно автору, модератору или администратору.
        /// </summary>
        /// <param name="id">Идентификатор статьи.</param>
        /// <param name="dto">Данные для обновления.</param>
        /// <returns>Результат операции.</returns>
        [HttpPut("{id:int}")]
        [Authorize(Policy = AppPermissions.ArticlesUpdate)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateArticleDto dto)
        {
            var result = await _articleService.UpdateAsync(id, dto, CurrentUserId, IsElevatedUser);
            return ToActionResult(result);
        }

        /// <summary>
        /// Удаляет статью. Доступно автору, модератору или администратору.
        /// </summary>
        /// <param name="id">Идентификатор статьи.</param>
        /// <returns>Результат операции.</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = AppPermissions.ArticlesDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _articleService.DeleteAsync(id, CurrentUserId, IsElevatedUser);
            return ToActionResult(result);
        }
    }
}
