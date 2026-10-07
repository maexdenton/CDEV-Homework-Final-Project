using System.Security.Claims;
using Blog.BLL.DTOs;
using Blog.BLL.Services;
using Blog.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    /// <summary>
    /// Контроллер публикаций. Обеспечивает полный CRUD статей, поиск и выборку по автору
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ArticleController : ControllerBase
    {
        private readonly IArticleService _articleService;
        private readonly ILogger<ArticleController> _logger;

        public ArticleController(IArticleService articleService, ILogger<ArticleController> logger)
        {
            _articleService = articleService;
            _logger = logger;
        }

        /// <summary>
        /// Создание новой статьи
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ArticleDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateArticleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _articleService.CreateAsync(dto);
            _logger.LogInformation("Создана статья ID={ArticleId} автором {AuthorId}", created.Id, created.AuthorId);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Получение списка всех статей с возможностью фильтрации по ключевым словам и тегу
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ArticleDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? tag)
        {
            var articles = await _articleService.GetAllAsync(search, tag);
            return Ok(articles);
        }

        /// <summary>
        /// Получение детальной информации о публикации, включая теги и список комментариев
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ArticleDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var article = await _articleService.GetByIdAsync(id);
            if (article == null)
                return NotFound(new { message = $"Статья с ID={id} не найдена." });

            return Ok(article);
        }

        /// <summary>
        /// Получение всех статей конкретного автора
        /// </summary>
        /// <param name="authorId">Идентификатор автора в Identity</param>
        [HttpGet("author/{authorId}")]
        [ProducesResponseType(typeof(IEnumerable<ArticleDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByAuthor(string authorId)
        {
            var articles = await _articleService.GetByAuthorIdAsync(authorId);
            return Ok(articles);
        }

        /// <summary>
        /// Редактирование статьи (заголовок, содержание, теги)
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateArticleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _articleService.UpdateAsync(id, dto);
            if (!updated)
                return NotFound(new { message = $"Статья с ID={id} не найдена." });

            return NoContent();
        }

        /// <summary>
        /// Удаление статьи по идентификатору
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _articleService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Статья с ID={id} не найдена." });

            _logger.LogWarning("Удалена статья ID={ArticleId}", id);
            return NoContent();
        }
    }
}
