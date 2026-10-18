using Blog.BLL.Security;
using Blog.DAL.Entities;
using Blog.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Web.Controllers
{
    // Контроллер управления пользователями через MVC-интерфейс (не API).
    // Политика RolesManage проверяется до входа в любой метод, поэтому все действия внутри защищены на уровне класса, а не отдельных экшенов.
    // Это исключает риск забыть атрибут на новом методе.
    [Authorize(Policy = AppPermissions.RolesManage)]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // Внедрение зависимостей через конструктор (DI)
        public UsersController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Users
        // Отображает список всех пользователей с их ролями
        public async Task<IActionResult> Index()
        {
            // Получаем всех пользователей из БД
            var users = await _userManager.Users.ToListAsync();
            var model = new List<UserListViewModel>();

            foreach (var user in users)
            {
                // Для каждого пользователя запрашиваем его роли
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

        // GET: Users/Details/5
        // Просмотр детальной информации о пользователе
        public async Task<IActionResult> Details(string id)
        {
            // Проверка на null защищает от запроса /Users/Details без параметра
            if (id == null) return NotFound();

            // Поиск пользователя по ID. Если не найден - возвращаем 404
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

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

        // GET: Users/Create
        // Отображение формы создания пользователя
        public async Task<IActionResult> Create()
        {
            // Список всех ролей загружается заранее, чтобы админ мог назначить их сразу при создании, не переходя на страницу редактирования
            var model = new UserCreateViewModel
            {
                AllRoles = await GetRolesAsync()
            };
            return View(model);
        }

        // POST: Users/Create
        // Обработка отправки формы создания
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от CSRF: без токена запрос будет отклонен
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (ModelState.IsValid) // Проверка валидности введенных данных
            {
                // Создаем сущность пользователя
                var user = new User
                {
                    // В качестве логина используется Email — стандартная практика для Identity,
                    // т.к. Email уникален и привычен пользователю
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    RegisteredAt = DateTime.UtcNow
                };

                // CreateAsync хеширует пароль и сохраняет пользователя в БД
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Роли назначаются только после успешного создания пользователя,
                    // чтобы не оставлять осиротевшие записи в промежуточной таблице
                    if (model.SelectedRoles.Any())
                    {
                        await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    }
                    return RedirectToAction(nameof(Index));
                }

                // Если Identity вернул ошибки (например, слабый пароль), добавляем их в ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Если дошли сюда, значит были ошибки валидации. 
            // Заново загружаем роли и возвращаем форму с данными пользователя
            model.AllRoles = await GetRolesAsync();
            return View(model);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Получаем текущие роли пользователя, чтобы отметить нужные чекбоксы
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

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserEditViewModel model)
        {
            // Проверка на подмену ID: маршрут и тело формы должны ссылаться на одного пользователя
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null) return NotFound();

                user.Email = model.Email;
                user.UserName = model.Email;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Логика обновления ролей (добавление/удаление) вырезана в текущей версии.
                    // При необходимости ее можно будет восстановить, сравнив текущие и выбранные роли.
                    return RedirectToAction(nameof(Index));
                }
                // ...
            }

            model.AllRoles = await GetRolesAsync();
            return View(model);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var model = new UserDeleteViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            return View(model);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Критическая защита: администратор не может удалить самого себя.
                // Без этой проверки можно случайно заблокировать единственный админский аккаунт
                // и потерять доступ к управлению системой. 
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

                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для получения названий всех ролей.
        // Вынесен, чтобы не дублировать запрос в Create (GET/POST) и Edit (GET/POST).
        private async Task<IEnumerable<string>> GetRolesAsync()
        {
            return await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
        }
    }
}
