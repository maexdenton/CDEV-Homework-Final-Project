using System.Security.Claims;
using Blog.BLL.DTOs;
using Blog.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    /// <summary>
    /// Контроллер пользовательского веб-интерфейса блога
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly ICommentService _commentService;

        public HomeController(IArticleService articleService, ICommentService commentService)
        {
            _articleService = articleService;
            _commentService = commentService;
        }

        /// <summary>
        /// Главная страница блога: лента статей, поиск и фильтр по тегам
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? tag)
        {
            ViewData["SearchQuery"] = search;
            ViewData["CurrentTag"] = tag;
            var articles = await _articleService.GetAllAsync(search, tag);
            return View(articles);
        }

        /// <summary>
        /// Просмотр статьи и комментариев к ней
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var article = await _articleService.GetByIdAsync(id);
            if (article == null) return NotFound();

            return View(article);
        }

        /// <summary>
        /// Страница создания новой статьи
        /// </summary>
        [Authorize]
        [HttpGet]
        public IActionResult Create() => View();

        /// <summary>
        /// Обработка создания статьи через форму
        /// </summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, string content, string tags)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("", "Заголовок и текст обязательны для заполнения.");
                return View();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var tagList = tags?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? new List<string>();

            var dto = new CreateArticleDto
            {
                Title = title,
                Content = content,
                AuthorId = userId,
                Tags = tagList
            };

            var created = await _articleService.CreateAsync(dto);
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }

        /// <summary>
        /// Добавление комментария к статье
        /// </summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int articleId, string content)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _commentService.CreateAsync(new CreateCommentDto
                {
                    ArticleId = articleId,
                    AuthorId = userId,
                    Content = content
                });
            }
            return RedirectToAction(nameof(Details), new { id = articleId });
        }
    }
}
