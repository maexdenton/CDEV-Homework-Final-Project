using System.Security.Claims;
using Blog.BLL.DTOs;
using Blog.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers.Api
{
    [ApiController]
    [Route("api/articles")]
    [Produces("application/json")]
    public class ArticlesApiController : ControllerBase
    {
        private readonly IArticleService _articleService;

        public ArticlesApiController(IArticleService articleService) => _articleService = articleService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArticleDto>>> GetArticles([FromQuery] string? search, [FromQuery] string? tag) =>
            Ok(await _articleService.GetArticlesAsync(search, tag));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ArticleDetailsDto>> GetArticle(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            return article == null ? NotFound() : Ok(article);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create([FromBody] CreateArticleDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var fixedDto = dto with { AuthorId = userId };
            var id = await _articleService.CreateArticleAsync(fixedDto);
            return CreatedAtAction(nameof(GetArticle), new { id }, fixedDto);
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var success = await _articleService.DeleteArticleAsync(id, userId);
            return success ? NoContent() : Forbid();
        }
    }
}
