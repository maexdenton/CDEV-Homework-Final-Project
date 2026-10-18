using Blog.BLL.DTOs;
using Blog.BLL.Services;
using Blog.DAL.Entities;
using Blog.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    // MVC-контроллер для аутентификации и управления учетной записью
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        // Зависимости внедряются через DI
        public AccountController(
            IUserService userService,
            SignInManager<User> signInManager,
            UserManager<User> userManager)
        {
            _userService = userService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // GET: /Account/Register
        // Если пользователь уже авторизован, форма регистрации не нужна — сразу идем на главную
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated ?? false)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: /Account/Register
        // Вся валидация и создание пользователя вынесены в BLL.
        // Контроллер только маппит ViewModel -> DTO и обрабатывает результат.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = new RegisterUserDto
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword
            };

            // Вызов BLL: сервис возвращает типизированный ServiceResult<UserDto>
            var result = await _userService.RegisterAsync(dto);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Ошибка при регистрации.");
                return View(model);
            }

            // После успешной регистрации сразу выполняем вход,
            // чтобы пользователю не пришлось вводить credentials повторно
            var user = await _userManager.FindByIdAsync(result.Data!.Id);
            if (user != null)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Login
        // returnUrl сохраняется в ViewData, чтобы форма могла передать его обратно
        // после успешного входа (например, при редиректе из [Authorize]-метода) 
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated ?? false)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            // lockoutOnFailure: false — временно отключена блокировка аккаунта
            // после нескольких неудачных попыток. Если понадобится защита от брутфорса,
            // это поведение можно изменить в настройках Identity (если когда-нибудь понадобится)
            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                isPersistent: true,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Безопасность: проверка Url.IsLocalUrl защищает от open redirect
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Неверный адрес электронной почты или пароль.");
            return View(model);
        }


        // GET: /Account/ChangePassword
        // Доступно только авторизованным пользователям
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Account/ChangePassword
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Пользователь удален или Cookie устарел - отправляем на повторный вход
                return RedirectToAction("Login", "Account");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    // Перехватываем стандартную ошибку Identity и заменяем ее на понятное сообщение,
                    // остальные ошибки (например, несоответствие политике паролей) выводим как есть 
                    if (error.Code == "PasswordMismatch")
                    {
                        ModelState.AddModelError(string.Empty, "Неверный текущий пароль");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                return View(model);
            }

            // Обновляем Security Stamp в Cookie, чтобы сессия осталась валидной
            // после смены пароля. Без этого пользователя может выкинуть из системы
            // при следующей проверке безопасности 
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Ваш пароль был успешно изменен.";
            return RedirectToAction("Index", "Home");
        }


        // POST: /Account/Logout
        // Выход выполняется только через POST с antiforgery-токеном,
        // чтобы предотвратить CSRF-атаки, при которых злоумышленник может
        // разлогинить пользователя через скрытую форму или изображение
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        // Вызывается middleware авторизации, когда пользователь аутентифицирован,
        // но у него нет прав на запрошенный ресурс (HTTP 403) 
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
