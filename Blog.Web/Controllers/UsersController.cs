using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.BLL.Security;
using Blog.DAL.Entities;
using Blog.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Blog.Web.Controllers
{
    /// <summary>
    /// MVC-контроллер управления пользователями через административный интерфейс.
    /// Доступен только пользователям с правом <see cref="AppPermissions.RolesManage"/> (администраторам).
    /// </summary>
    [Authorize(Policy = AppPermissions.RolesManage)]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        /// <summary>
        /// Отображает список всех пользователей с их ролями.
        /// </summary>
        /// <returns>Представление со списком пользователей.</returns>
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new List<UserListViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserListViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles
                });
            }
            return View(model);
        }

        /// <summary>
        /// Отображает детальную информацию о пользователе.
        /// </summary>
        /// <param name="id">Идентификатор пользователя.</param>
        /// <returns>Представление с деталями пользователя или 404, если пользователь не найден.</returns>
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var model = new UserDetailsViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                RegisteredAt = user.RegisteredAt,
                Roles = roles
            };
            return View(model);
        }

        /// <summary>
        /// Отображает форму создания нового пользователя.
        /// </summary>
        /// <returns>Представление с формой создания.</returns>
        public async Task<IActionResult> Create()
        {
            var model = new UserCreateViewModel
            {
                AllRoles = await GetRolesAsync()
            };
            return View(model);
        }

        /// <summary>
        /// Обрабатывает создание нового пользователя и назначает выбранные роли.
        /// Все действия администратора фиксируются в логе.
        /// </summary>
        /// <param name="model">Данные нового пользователя из формы.</param>
        /// <returns>Перенаправление к списку при успехе или представление с ошибками.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    RegisteredAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var adminName = User.Identity?.Name ?? "Unknown Admin";
                    if (model.SelectedRoles.Any())
                    {
                        var roleResult = await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                        if (roleResult.Succeeded)
                        {
                            _logger.LogInformation(
                                "Admin {Admin} created user {TargetUser} and assigned roles: {Roles}",
                                adminName, user.Email, string.Join(", ", model.SelectedRoles));
                        }
                        else
                        {
                            _logger.LogError(
                                "Admin {Admin} created user {TargetUser}, but failed to assign roles: {Errors}",
                                adminName, user.Email,
                                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                        }
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Admin {Admin} created user {TargetUser} without any roles.",
                            adminName, user.Email);
                    }
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.AllRoles = await GetRolesAsync();
            return View(model);
        }

        /// <summary>
        /// Отображает форму редактирования пользователя.
        /// </summary>
        /// <param name="id">Идентификатор пользователя.</param>
        /// <returns>Представление с формой редактирования или 404, если пользователь не найден.</returns>
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var model = new UserEditViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                SelectedRoles = userRoles.ToList(),
                AllRoles = await GetRolesAsync()
            };
            return View(model);
        }

        /// <summary>
        /// Обрабатывает редактирование профиля и изменение ролей пользователя.
        /// Изменения ролей логируются отдельно для каждой добавленной и удаленной роли,
        /// чтобы обеспечить прослеживаемость действий администратора.
        /// </summary>
        /// <param name="id">Идентификатор пользователя.</param>
        /// <param name="model">Данные формы редактирования.</param>
        /// <returns>Перенаправление к списку при успехе или представление с ошибками.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserEditViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                    return NotFound();

                user.Email = model.Email;
                user.UserName = model.Email;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    var adminName = User.Identity?.Name ?? "Unknown Admin";
                    var currentRoles = await _userManager.GetRolesAsync(user);

                    // Вычисляем разницу между текущими и выбранными ролями, чтобы
                    // логировать только фактически выполненные операции.
                    var rolesToAdd = model.SelectedRoles.Except(currentRoles).ToList();
                    var rolesToRemove = currentRoles.Except(model.SelectedRoles).ToList();

                    if (rolesToAdd.Any())
                    {
                        var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                        if (addResult.Succeeded)
                        {
                            foreach (var role in rolesToAdd)
                            {
                                _logger.LogInformation(
                                    "Admin {Admin} assigned role {Role} to user {TargetUser}.",
                                    adminName, role, user.Email);
                            }
                        }
                        else
                        {
                            _logger.LogError(
                                "Failed to add roles to {TargetUser} by Admin {Admin}: {Errors}",
                                user.Email, adminName,
                                string.Join(", ", addResult.Errors.Select(e => e.Description)));
                        }
                    }

                    if (rolesToRemove.Any())
                    {
                        var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                        if (removeResult.Succeeded)
                        {
                            foreach (var role in rolesToRemove)
                            {
                                _logger.LogInformation(
                                    "Admin {Admin} removed role {Role} from user {TargetUser}.",
                                    adminName, role, user.Email);
                            }
                        }
                        else
                        {
                            _logger.LogError(
                                "Failed to remove roles from {TargetUser} by Admin {Admin}: {Errors}",
                                user.Email, adminName,
                                string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                        }
                    }

                    if (!rolesToAdd.Any() && !rolesToRemove.Any())
                    {
                        _logger.LogInformation(
                            "Admin {Admin} updated profile of user {TargetUser} (no role changes).",
                            adminName, user.Email);
                    }

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.AllRoles = await GetRolesAsync();
            return View(model);
        }

        /// <summary>
        /// Отображает страницу подтверждения удаления пользователя.
        /// </summary>
        /// <param name="id">Идентификатор пользователя.</param>
        /// <returns>Представление подтверждения или 404, если пользователь не найден.</returns>
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new UserDeleteViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
            return View(model);
        }

        /// <summary>
        /// Выполняет удаление пользователя. Запрещает администратору удалять собственную учетную запись.
        /// Действие фиксируется в логе.
        /// </summary>
        /// <param name="id">Идентификатор удаляемого пользователя.</param>
        /// <returns>Перенаправление к списку пользователей.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Защита от удаления самого себя: администратор не должен лишать
                // систему единственной учетной записи с полными правами.
                var currentUserId = _userManager.GetUserId(User);
                if (user.Id == currentUserId)
                {
                    ModelState.AddModelError(string.Empty, "Вы не можете удалить собственную учетную запись.");
                    var model = new UserDeleteViewModel
                    {
                        Id = user.Id,
                        Email = user.Email ?? string.Empty,
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    };
                    return View(model);
                }

                var adminName = User.Identity?.Name ?? "Unknown Admin";
                var targetEmail = user.Email;
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Admin {Admin} deleted user {TargetUser}.", adminName, targetEmail);
                }
                else
                {
                    _logger.LogError(
                        "Admin {Admin} failed to delete user {TargetUser}: {Errors}",
                        adminName, targetEmail,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Возвращает список названий всех ролей в системе.
        /// </summary>
        /// <returns>Список названий ролей.</returns>
        private async Task<List<string>> GetRolesAsync()
        {
            return await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
        }
    }
}
