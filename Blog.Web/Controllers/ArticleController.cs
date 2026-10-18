using Blog.BLL.DTOs;
using Blog.BLL.Security;
using Blog.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    // Контроллер публикаций. Обеспечивает полный CRUD статей, поиск и выборку по автору
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ArticleController : BaseApiController
    {
        private readonly IArticleService _articleService;

        public ArticleController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? tag) =>
            Ok(await _articleService.GetAllAsync(search, tag));

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var article = await _articleService.GetByIdAsync(id);
            return article == null ? NotFound(new { message = "Статья не найдена." }) : Ok(article);
        }

        [HttpGet("author/{authorId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByAuthor(string authorId) =>
            Ok(await _articleService.GetByAuthorIdAsync(authorId));

        // Создание статьи. Требуется право Articles.Create
        [HttpPost]
        [Authorize(Policy = AppPermissions.ArticlesCreate)]
        [ProducesResponseType(typeof(ArticleDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateArticleDto dto)
        {
            // Передаем CurrentUserId из токена/куки, исключая подмену автора в теле запроса
            var created = await _articleService.CreateAsync(dto, CurrentUserId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // Редактирование статьи. Доступно автору, Модератору и Администратору
        [HttpPut("{id:int}")]
        [Authorize(Policy = AppPermissions.ArticlesUpdate)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateArticleDto dto)
        {
            var result = await _articleService.UpdateAsync(id, dto, CurrentUserId, IsElevatedUser);
            return ToActionResult(result);
        }

        // Удаление статьи. Доступно автору, Модератору и Администратору
        [HttpDelete("{id:int}")]
        [Authorize(Policy = AppPermissions.ArticlesDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _articleService.DeleteAsync(id, CurrentUserId, IsElevatedUser);
            return ToActionResult(result);
        }
    }
}
