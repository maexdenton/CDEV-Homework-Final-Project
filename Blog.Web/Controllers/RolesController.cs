using Blog.BLL.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blog.Web.ViewModels;

namespace Blog.Web.Controllers
{
    // Контроллер управления ролями. 
    // Доступен только пользователям с правом RolesManage строго для администратора. Политика проверяется до входа в любой метод.
    [Authorize(Policy = AppPermissions.RolesManage)]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        // Зависимость только от RoleManager, т.к. контроллер работает исключительно с ролями
        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        // Проверяет, является ли роль одной из трех предустановленных системных ролей.
        // Такие роли нельзя переименовывать или удалять, чтобы не сломать логику авторизации,
        // которая завязана на константы AppRoles.Administrator / Moderator / User.
        private bool IsSystemRole(string roleName)
        {
            return roleName == AppRoles.Administrator ||
                   roleName == AppRoles.Moderator ||
                   roleName == AppRoles.User;
        }

        // GET: Roles
        // Список всех ролей. Вьюха сама решает, показывать ли кнопки редактирования/удаления,
        // но финальная защита всt равно остаtтся на уровне контроллера (в Edit/Delete).
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

        // GET: Roles/Details/5
        // Детальный просмотр роли. Если роль не найдена — 404
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            var model = new RoleListViewModel
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty
            };

            return View(model);
        }

        // GET: Roles/Edit/5
        // Отображение формы редактирования.
        // Если роль системная — вместо формы возвращается страница Details с сообщением об ошибке.
        // Это осознанное решение: пользователь видит детали роли и понимает, почему редактирование недоступно.
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            // Блокируем редактирование системных ролей на уровне контроллера
            if (IsSystemRole(role.Name!))
            {
                ModelState.AddModelError(string.Empty, "Системную роль нельзя редактировать.");
                return View("Details", new RoleListViewModel { Id = role.Id, Name = role.Name ?? string.Empty });
            }

            var model = new RoleEditViewModel
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty
            };

            return View(model);
        }

        // POST: Roles/Edit/5
        // Повторная проверка на системную роль обязательна: злоумышленник может отправить POST напрямую,
        // минуя GET-форму. Без этой проверки системную роль можно было бы переименовать
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, RoleEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            // Дополнительная защита на уровне POST-запроса
            if (IsSystemRole(role.Name!))
            {
                ModelState.AddModelError(string.Empty, "Системную роль нельзя редактировать.");
                return View("Details", new RoleListViewModel { Id = role.Id, Name = role.Name ?? string.Empty });
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

        // GET: Roles/Delete/5
        // Аналогично Edit: для системных ролей показываем страницу Details с сообщением, а не форму подтверждения удаления
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            // Блокируем удаление системных ролей
            if (IsSystemRole(role.Name!))
            {
                ModelState.AddModelError(string.Empty, "Системную роль нельзя удалить.");
                return View("Details", new RoleListViewModel { Id = role.Id, Name = role.Name ?? string.Empty });
            }

            var model = new RoleDeleteViewModel
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty
            };

            return View(model);
        }

        // POST: Roles/Delete/5
        // Финальная защита от удаления системной роли. Даже если запрос отправлен напрямую,
        // роль не будет удалена. Дополнительно обрабатываются ошибки Identity (например, если роль
        // все еще назначена пользователям, Identity может запретить удаление).
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                if (IsSystemRole(role.Name!))
                {
                    ModelState.AddModelError(string.Empty, $"Системную роль «{role.Name}» удалить нельзя.");
                    return View("Details", new RoleListViewModel { Id = role.Id, Name = role.Name ?? string.Empty });
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
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
