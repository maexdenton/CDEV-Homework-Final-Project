using Blog.BLL.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blog.Web.ViewModels;

namespace Blog.Web.Controllers
{
    // Доступ к управлению ролями строго для администратора
    [Authorize(Policy = AppPermissions.RolesManage)]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        // Вспомогательный метод для проверки, является ли роль системной
        private bool IsSystemRole(string roleName)
        {
            return roleName == AppRoles.Administrator ||
                   roleName == AppRoles.Moderator ||
                   roleName == AppRoles.User;
        }

        // GET: Roles
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
