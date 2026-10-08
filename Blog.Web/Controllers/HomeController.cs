using System.Security.Claims;
using Blog.BLL.DTOs;
using Blog.BLL.Security;
using Blog.BLL.Services;
using Blog.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    /// <summary>
    /// Контроллер веб-интерфейса блога с поддержкой полного жизненного цикла статей и комментариев
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
        /// Главная страница: вывод ленты статей с фильтрацией по поиску и тегу
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
        /// Просмотр статьи и комментариев
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var article = await _articleService.GetByIdAsync(id);
            if (article == null) return NotFound();

            return View(article);
        }

        /// <summary>
        /// Страница создания новой статьи (только для авторизованных)
        /// </summary>
        [Authorize]
        [HttpGet]
        public IActionResult Create() => View();

        /// <summary>
        /// Обработка отправки формы создания статьи
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

            // Идентификатор автора строго берется из Claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var tagList = tags?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? new List<string>();

            var dto = new CreateArticleDto
            {
                Title = title,
                Content = content,
                AuthorId = userId,
                Tags = tagList
            };

            var created = await _articleService.CreateAsync(dto, userId);
            TempData["SuccessMessage"] = "Статья успешно опубликована!";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }

        /// <summary>
        /// Страница редактирования статьи.
        /// Доступна автору, модератору и администратору
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var article = await _articleService.GetByIdAsync(id);
            if (article == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isElevated = User.IsInRole(AppRoles.Administrator) || User.IsInRole(AppRoles.Moderator);

            // Проверка прав на отображение формы
            if (article.AuthorId != userId && !isElevated)
            {
                return Forbid();
            }

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
        /// Обработка сохранения изменений статьи
        /// </summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ArticleEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isElevated = User.IsInRole(AppRoles.Administrator) || User.IsInRole(AppRoles.Moderator);

            var tagList = model.Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? new List<string>();

            var dto = new UpdateArticleDto
            {
                Title = model.Title,
                Content = model.Content,
                Tags = tagList
            };

            // Сервис проверяет права и владение
            var result = await _articleService.UpdateAsync(id, dto, userId, isElevated);

            if (!result.Succeeded)
            {
                if (result.Status == ResultStatus.Forbidden) return Forbid();
                ModelState.AddModelError("", result.Message ?? "Не удалось обновить публикацию.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Статья успешно обновлена.";
            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// Удаление статьи с проверкой прав на стороне сервиса
        /// </summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isElevated = User.IsInRole(AppRoles.Administrator) || User.IsInRole(AppRoles.Moderator);

            var result = await _articleService.DeleteAsync(id, userId, isElevated);

            if (!result.Succeeded)
            {
                if (result.Status == ResultStatus.Forbidden) return Forbid();
                TempData["ErrorMessage"] = result.Message ?? "Ошибка при удалении статьи.";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = "Публикация была успешно удалена.";
            return RedirectToAction(nameof(Index));
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
                var result = await _commentService.CreateAsync(new CreateCommentDto
                {
                    ArticleId = articleId,
                    AuthorId = userId,
                    Content = content
                }, userId);

                if (result.Succeeded)
                {
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
        /// Редактирование текста комментария прямо на странице статьи
        /// </summary>
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isElevated = User.IsInRole(AppRoles.Administrator) || User.IsInRole(AppRoles.Moderator);

            var result = await _commentService.UpdateAsync(commentId, new UpdateCommentDto { Content = content }, userId, isElevated);

            if (!result.Succeeded)
            {
                if (result.Status == ResultStatus.Forbidden) return Forbid();
                TempData["ErrorMessage"] = result.Message ?? "Не удалось изменить комментарий.";
            }
            else
            {
                TempData["SuccessMessage"] = "Комментарий успешно обновлен.";
            }

            return RedirectToAction(nameof(Details), new { id = articleId });
        }

        /// <summary>
        /// Удаление комментария прямо на странице статьи
        /// </summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId, int articleId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isElevated = User.IsInRole(AppRoles.Administrator) || User.IsInRole(AppRoles.Moderator);

            var result = await _commentService.DeleteAsync(commentId, userId, isElevated);

            if (!result.Succeeded)
            {
                if (result.Status == ResultStatus.Forbidden) return Forbid();
                TempData["ErrorMessage"] = result.Message ?? "Не удалось удалить комментарий.";
            }
            else
            {
                TempData["SuccessMessage"] = "Комментарий удален.";
            }

            return RedirectToAction(nameof(Details), new { id = articleId });
        }
    }
}
