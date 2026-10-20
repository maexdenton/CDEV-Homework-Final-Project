using System.Security.Claims;
using Blog.BLL.DTOs;
using Blog.BLL.Security;
using Blog.BLL.Services;
using Blog.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Blog.Web.Controllers
{
    /// <summary>
    /// MVC-контроллер веб-интерфейса блога.
    /// Отвечает за отображение ленты статей, просмотр, создание, редактирование
    /// и удаление публикаций, а также за управление комментариями прямо со страницы статьи.
    /// Вся бизнес-логика (проверка прав, валидация, работа с БД) делегируется
    /// в сервисный слой (<see cref="IArticleService"/>, <see cref="ICommentService"/>).
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly ICommentService _commentService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IArticleService articleService,
            ICommentService commentService,
            ILogger<HomeController> logger)
        {
            _articleService = articleService;
            _commentService = commentService;
            _logger = logger;
        }

        /// <summary>
        /// Главная страница: лента статей с фильтрацией по поисковому запросу и тегу.
        /// </summary>
        /// <param name="search">Поисковая строка (необязательно).</param>
        /// <param name="tag">Название тега для фильтрации (необязательно).</param>
        /// <returns>Представление с лентой статей.</returns>
        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? tag)
        {
            ViewData["SearchQuery"] = search;
            ViewData["CurrentTag"] = tag;
            var articles = await _articleService.GetAllAsync(search, tag);
            return View(articles);
        }

        /// <summary>
        /// Просмотр конкретной статьи и всех ее комментариев.
        /// </summary>
        /// <param name="id">Идентификатор статьи.</param>
        /// <returns>Представление статьи или 404, если статья не найдена.</returns>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var article = await _articleService.GetByIdAsync(id);
            if (article == null)
                return NotFound();

            return View(article);
        }

        /// <summary>
        /// Отображает форму создания новой статьи.
        /// Доступно только аутентифицированным пользователям.
        /// </summary>
        /// <returns>Представление с формой создания.</returns>
        [Authorize]
        [HttpGet]
        public IActionResult Create() => View(new ArticleCreateViewModel());

        /// <summary>
        /// Обрабатывает создание новой статьи.
        /// Автор статьи определяется по текущей сессии, а не по данным из формы.
        /// </summary>
        /// <param name="model">Данные статьи из формы.</param>
        /// <returns>Перенаправление на созданную статью или представление с ошибками.</returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArticleCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var tagList = model.Tags?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList() ?? new List<string>();

            var dto = new CreateArticleDto
            {
                Title = model.Title,
                Content = model.Content,
                AuthorId = userId,
                Tags = tagList
            };

            var created = await _articleService.CreateAsync(dto, userId);

            _logger.LogInformation("User {User} created a new article titled '{Title}'",
                User.Identity?.Name, model.Title);

            TempData["SuccessMessage"] = "Статья успешно опубликована!";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }

        /// <summary>
        /// Отображает форму редактирования статьи.
        /// Доступно автору, модератору или администратору.
        /// </summary>
        /// <param name="id">Идентификатор статьи.</param>
        /// <returns>Представление с формой редактирования, 404 или 403.</returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var article = await _articleService.GetByIdAsync(id);
            if (article == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isElevated = User.IsInRole(AppRoles.Administrator) || User.IsInRole(AppRoles.Moderator);

            if (article.AuthorId != userId && !isElevated)
                return Forbid();

            var model = new ArticleEditViewModel
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                Tags = string.Join(", ", article.Tags)
            };

            return View(model);
        }

        /// <summary>
        /// Обрабатывает редактирование статьи.
        /// Доступно автору, модератору или администратору.
        /// </summary>
        /// <param name="id">Идентификатор статьи.</param>
        /// <param name="model">Данные статьи из формы.</param>
        /// <returns>Перенаправление на статью или представление с ошибками.</returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ArticleEditViewModel model)
        {
            var article = await _articleService.GetByIdAsync(id);
            if (article == null)
                return NotFound();

            // Обход валидации для старых данных: если заголовок короче 5 символов,
            // но пользователь его не менял, ошибка валидации не должна блокировать сохранение.
            if (article.Title == model.Title && article.Title.Length < 5)
            {
                ModelState.Remove(nameof(model.Title));
            }

            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isElevated = User.IsInRole(AppRoles.Administrator) || User.IsInRole(AppRoles.Moderator);

            var tagList = model.Tags?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList() ?? new List<string>();

            var dto = new UpdateArticleDto
            {
                Title = model.Title,
                Content = model.Content,
                Tags = tagList
            };

            var result = await _articleService.UpdateAsync(id, dto, userId, isElevated);
            if (!result.Succeeded)
            {
                if (result.Status == ResultStatus.Forbidden)
                    return Forbid();

                ModelState.AddModelError("", result.Message ?? "Не удалось обновить публикацию.");
                return View(model);
            }

            // Фиксируем в логе, какие именно поля изменились, — это упрощает аудит действий пользователя.
            var changes = new List<string>();
            if (article.Title != model.Title)
                changes.Add($"Title: '{article.Title}' -> '{model.Title}'");
            if (article.Content != model.Content)
                changes.Add("Content was modified");

            _logger.LogInformation("User {User} edited article '{Title}'. Changes: {Changes}",
                User.Identity?.Name, model.Title, string.Join("; ", changes));

            TempData["SuccessMessage"] = "Статья успешно обновлена.";
            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// Удаляет статью.
        /// Доступно автору, модератору или администратору.
        /// </summary>
        /// <param name="id">Идентификатор статьи.</param>
        /// <returns>Перенаправление на ленту статей или на страницу статьи при ошибке.</returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _articleService.GetByIdAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isElevated = User.IsInRole(AppRoles.Administrator) || User.IsInRole(AppRoles.Moderator);

            var result = await _articleService.DeleteAsync(id, userId, isElevated);
            if (!result.Succeeded)
            {
                if (result.Status == ResultStatus.Forbidden)
                    return Forbid();

                TempData["ErrorMessage"] = result.Message ?? "Ошибка при удалении статьи.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _logger.LogInformation("User {User} deleted article '{Title}'",
                User.Identity?.Name, article?.Title);

            TempData["SuccessMessage"] = "Публикация была успешно удалена.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Добавляет комментарий к статье от имени текущего пользователя.
        /// </summary>
        /// <param name="articleId">Идентификатор статьи.</param>
        /// <param name="content">Текст комментария.</param>
        /// <returns>Перенаправление на страницу статьи.</returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int articleId, string content)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _commentService.CreateAsync(
                    new CreateCommentDto
                    {
                        ArticleId = articleId,
                        AuthorId = userId,
                        Content = content
                    }, userId);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {User} added a comment to article {ArticleId}. Text: '{Content}'",
                        User.Identity?.Name, articleId, content);

                    TempData["SuccessMessage"] = "Комментарий успешно добавлен.";
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }

            return RedirectToAction(nameof(Details), new { id = articleId });
        }

        /// <summary>
        /// Редактирует комментарий.
        /// Доступно автору, модератору или администратору.
        /// </summary>
        /// <param name="commentId">Идентификатор комментария.</param>
        /// <param name="articleId">Идентификатор статьи, к которой относится комментарий.</param>
        /// <param name="content">Новый текст комментария.</param>
        /// <returns>Перенаправление на страницу статьи.</returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(int commentId, int articleId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Текст комментария не может быть пустым.";
                return RedirectToAction(nameof(Details), new { id = articleId });
            }

            // Старый текст нужен только для лога: в нем фиксируется, что именно изменилось.
            var oldComment = await _commentService.GetByIdAsync(commentId);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isElevated = User.IsInRole(AppRoles.Administrator) || User.IsInRole(AppRoles.Moderator);

            var result = await _commentService.UpdateAsync(
                commentId,
                new UpdateCommentDto { Content = content },
                userId,
                isElevated);

            if (!result.Succeeded)
            {
                if (result.Status == ResultStatus.Forbidden)
                    return Forbid();

                TempData["ErrorMessage"] = result.Message ?? "Не удалось изменить комментарий.";
            }
            else
            {
                _logger.LogInformation("User {User} edited comment {CommentId}. Old text: '{OldText}', New text: '{NewText}'",
                    User.Identity?.Name, commentId, oldComment?.Content, content);

                TempData["SuccessMessage"] = "Комментарий успешно обновлен.";
            }

            return RedirectToAction(nameof(Details), new { id = articleId });
        }

        /// <summary>
        /// Удаляет комментарий.
        /// Доступно автору, модератору или администратору.
        /// </summary>
        /// <param name="commentId">Идентификатор комментария.</param>
        /// <param name="articleId">Идентификатор статьи, к которой относится комментарий.</param>
        /// <returns>Перенаправление на страницу статьи.</returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId, int articleId)
        {
            // Текст удаляемого комментария нужен только для лога — после удаления восстановить его будет нельзя.
            var commentToDelete = await _commentService.GetByIdAsync(commentId);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isElevated = User.IsInRole(AppRoles.Administrator) || User.IsInRole(AppRoles.Moderator);

            var result = await _commentService.DeleteAsync(commentId, userId, isElevated);
            if (!result.Succeeded)
            {
                if (result.Status == ResultStatus.Forbidden)
                    return Forbid();

                TempData["ErrorMessage"] = result.Message ?? "Не удалось удалить комментарий.";
            }
            else
            {
                _logger.LogInformation("User {User} deleted comment {CommentId}. Deleted text: '{Content}'",
                    User.Identity?.Name, commentId, commentToDelete?.Content);

                TempData["SuccessMessage"] = "Комментарий удален.";
            }

            return RedirectToAction(nameof(Details), new { id = articleId });
        }
    }
}
