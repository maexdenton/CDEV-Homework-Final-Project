using System.Security.Claims;
using Blog.BLL.DTOs;
using Blog.BLL.Security;
using Blog.BLL.Services;
using Blog.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    // MVC-контроллер веб-интерфейса блога.
    // Отвечает за отображение ленты статей, просмотр, создание, редактирование и удаление публикаций, а также за управление комментариями прямо со страницы статьи.
    // Вся бизнес-логика (проверка прав, валидация, работа с БД) делегируется в сервисный слой (<see cref="IArticleService"/>, <see cref="ICommentService"/>).
    public class HomeController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly ICommentService _commentService;

        // Сервисы внедряются через DI. Контроллер не работает с DbContext напрямую,
        // что упрощает тестирование и сохраняет разделение слоев 
        public HomeController(IArticleService articleService, ICommentService commentService)
        {
            _articleService = articleService;
            _commentService = commentService;
        }

        // Главная страница: лента статей с фильтрацией по поисковому запросу и тегу.
        // Параметры <paramref name="search"/> и <paramref name="tag"/> передаются в сервис, который применяет соответствующую фильтрацию к запросу.
        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? tag)
        {
            ViewData["SearchQuery"] = search;
            ViewData["CurrentTag"] = tag;
            var articles = await _articleService.GetAllAsync(search, tag);
            return View(articles);
        }

        // Просмотр конкретной статьи и всех ее комментариев.
        // Если статья не найдена — возвращается 404 (страница "Ресурс не найден").
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var article = await _articleService.GetByIdAsync(id);
            if (article == null) return NotFound();

            return View(article);
        }

        // Отображение формы создания новой статьи.
        // Доступно только авторизованным пользователям.
        [Authorize]
        [HttpGet]
        public IActionResult Create() => View();

        // Обработка отправки формы создания статьи.
        // Идентификатор автора строго берется из Сlaims текущей сессии, а не из данных формы — это исключает подмену авторства.
        // Теги, введеные через запятую, разбиваются на список и передаются в сервис.
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

            // Автор определяется по аутентифицированному пользователю (строго берется из Claims)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var tagList = tags?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? new List<string>();

            // Пользователь может ввести теги через запятую (пустые и лишние пробелы удаляются)
            var dto = new CreateArticleDto
            {
                Title = title,
                Content = content,
                AuthorId = userId,
                Tags = tagList
            };

            var created = await _articleService.CreateAsync(dto, userId);
            TempData["SuccessMessage"] = "Статья успешно опубликована!";

            // После создания перенаправляем на страницу просмотра новой статьи
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }

        // Страница редактирования статьи.
        // Доступна автору, модератору и администратору.
        // Проверка прав выполняется здесь (для отображения формы) и повторно в сервисе (при сохранении), чтобы исключить обход через прямые POST-запросы.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var article = await _articleService.GetByIdAsync(id);
            if (article == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Модераторы и администраторы могут редактировать любые статьи
            var isElevated = User.IsInRole(AppRoles.Administrator) || User.IsInRole(AppRoles.Moderator);

            // Автор может редактировать только свои статьи, а остальным — 403
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

        // Обработка сохранения изменений статьи.
        // Финальная проверка прав выполняется в сервисном слое:
        // контроллер передает <paramref name="isElevated"/>, а сервис решает, разрешено ли обновление. При отказе возвращается 403.
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

            // Сервис проверяет права и владение статьей
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

        // Удаление статьи. Проверка прав выполняется на стороне сервиса.
        // При успехе пользователь возвращается на главную страницу, при ошибке — обратно на страницу статьи с сообщением в TempData.
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

        // Добавление комментария к статье.
        // Автор комментария берется из Сlaims, а не из формы.
        // После добавления пользователь возвращается на страницу статьи.
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

        // Редактирование текста комментария прямо на странице статьи.
        // Как и при редактировании статьи, проверка прав делегируется сервису.
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

        // Удаление комментария прямо на странице статьи.
        // Проверка прав (автор / модератор / администратор) выполняется в сервисе.
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
