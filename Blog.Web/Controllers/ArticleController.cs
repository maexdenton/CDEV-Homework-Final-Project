using System.Security.Claims;
using Blog.BLL.DTOs;
using Blog.BLL.Services;
using Blog.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    public class ArticleController : Controller
    {
        private readonly IArticleService _articleService;

        public ArticleController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? tag)
        {
            var articles = await _articleService.GetArticlesAsync(search, tag);
            return View(new ArticleIndexViewModel
            {
                Articles = articles,
                SearchQuery = search,
                TagFilter = tag
            });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null) return NotFound();

            return View(new ArticleDetailsPageViewModel { Article = article });
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create() => View(new ArticleCreateViewModel());

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArticleCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var tagList = model.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            var articleId = await _articleService.CreateArticleAsync(new CreateArticleDto(model.Title, model.Content, userId, tagList));
            return RedirectToAction(nameof(Details), new { id = articleId });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int articleId, ArticleDetailsPageViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.NewCommentContent))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _articleService.AddCommentAsync(new CreateCommentDto(articleId, model.NewCommentContent, userId));
            }
            return RedirectToAction(nameof(Details), new { id = articleId });
        }
    }
}
