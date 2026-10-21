using Blog.BLL.Security;
using Blog.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Web.Controllers
{
    /// <summary>
    /// MVC-контроллер управления ролями. Доступен только администраторам
    /// (политика <see cref="AppPermissions.RolesManage"/>), проверяется до входа в любой метод.
    /// </summary>
    [Authorize(Policy = AppPermissions.RolesManage)]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        /// <summary>
        /// Проверяет, является ли роль одной из предустановленных системных ролей.
        /// Такие роли нельзя переименовывать или удалять, чтобы не сломать логику авторизации,
        /// завязанную на константы <see cref="AppRoles"/>.
        /// </summary>
        /// <param name="roleName">Имя проверяемой роли.</param>
        /// <returns><c>true</c>, если роль системная.</returns>
        private bool IsSystemRole(string roleName) =>
            roleName == AppRoles.Administrator ||
            roleName == AppRoles.Moderator ||
            roleName == AppRoles.User;

        /// <summary>
        /// Отображает список всех ролей.
        /// </summary>
        /// <returns>Представление со списком ролей.</returns>
        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var model = roles.Select(r => new RoleListViewModel
            {
                Id = r.Id,
                Name = r.Name ?? string.Empty
            }).ToList();

            return View(model);
        }

        /// <summary>
        /// Отображает детальную информацию о роли.
        /// </summary>
        /// <param name="id">Идентификатор роли.</param>
        /// <returns>Представление с деталями роли или 404, если роль не найдена.</returns>
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
                return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            var model = new RoleListViewModel
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty
            };
            return View(model);
        }

        /// <summary>
        /// Отображает форму редактирования роли. Для системных ролей вместо формы
        /// возвращается страница Details с сообщением об ошибке — пользователь видит детали
        /// роли и понимает, почему редактирование недоступно.
        /// </summary>
        /// <param name="id">Идентификатор роли.</param>
        /// <returns>Представление формы или страница Details для системной роли.</returns>
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
                return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            // Системные роли блокируются на уровне контроллера, а не только в UI.
            if (IsSystemRole(role.Name!))
            {
                ModelState.AddModelError(string.Empty, "Системную роль нельзя редактировать.");
                return View("Details", new RoleListViewModel
                {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty
                });
            }

            var model = new RoleEditViewModel
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty
            };
            return View(model);
        }

        /// <summary>
        /// Обрабатывает редактирование роли.
        /// Повторная проверка на системную роль обязательна: злоумышленник может отправить
        /// POST напрямую, минуя GET-форму, и без этой проверки системную роль можно было бы переименовать.
        /// </summary>
        /// <param name="id">Идентификатор роли.</param>
        /// <param name="model">Данные формы редактирования.</param>
        /// <returns>Перенаправление при успехе или представление с ошибками.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, RoleEditViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            // Дополнительная защита на уровне POST-запроса: GET-форму можно обойти.
            if (IsSystemRole(role.Name!))
            {
                ModelState.AddModelError(string.Empty, "Системную роль нельзя редактировать.");
                return View("Details", new RoleListViewModel
                {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty
                });
            }

            if (ModelState.IsValid)
            {
                role.Name = model.Name;
                var result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        /// <summary>
        /// Отображает страницу подтверждения удаления роли. Аналогично <see cref="Edit"/>:
        /// для системных ролей показывается страница Details с сообщением, а не форма удаления.
        /// </summary>
        /// <param name="id">Идентификатор роли.</param>
        /// <returns>Представление подтверждения или страница Details для системной роли.</returns>
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
                return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            // Удаление системных ролей блокируется на уровне контроллера.
            if (IsSystemRole(role.Name!))
            {
                ModelState.AddModelError(string.Empty, "Системную роль нельзя удалить.");
                return View("Details", new RoleListViewModel
                {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty
                });
            }

            var model = new RoleDeleteViewModel
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty
            };
            return View(model);
        }

        /// <summary>
        /// Выполняет удаление роли. Даже при прямом POST системная роль не будет удалена.
        /// Ошибки Identity (например, если роль все еще назначена пользователям) выводятся в ModelState.
        /// </summary>
        /// <param name="id">Идентификатор удаляемой роли.</param>
        /// <returns>Перенаправление к списку или представление с ошибками.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return RedirectToAction(nameof(Index));

            // Финальная защита: даже прямой POST не удалит системную роль.
            if (IsSystemRole(role.Name!))
            {
                ModelState.AddModelError(string.Empty, $"Системную роль «{role.Name}» удалить нельзя.");
                return View("Details", new RoleListViewModel
                {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty
                });
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(new RoleDeleteViewModel { Id = role.Id, Name = role.Name });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
