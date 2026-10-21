using Blog.BLL.DTOs;
using Blog.BLL.Services;
using Blog.DAL.Entities;
using Blog.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    /// <summary>
    /// MVC-контроллер для аутентификации и управления учетной записью пользователя.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IUserService userService,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            ILogger<AccountController> logger)
        {
            _userService = userService;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Отображает страницу регистрации. Если пользователь уже аутентифицирован, перенаправляет на главную.
        /// </summary>
        /// <returns>Представление регистрации или перенаправление.</returns>
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated ?? false)
                return RedirectToAction("Index", "Home");

            return View();
        }

        /// <summary>
        /// Обрабатывает регистрацию нового пользователя.
        /// </summary>
        /// <param name="model">Данные регистрации из формы.</param>
        /// <returns>Перенаправление при успехе или представление с ошибками валидации.</returns>
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

            var result = await _userService.RegisterAsync(dto);
            if (!result.Succeeded)
            {
                // Ошибки бизнес-логики (например, занятый email) не являются системными сбоями,
                // поэтому логируются как Warning, а не Error.
                _logger.LogWarning("Registration failed for {Email}. Reason: {Reason}",
                    model.Email, result.Message ?? "Unknown error");

                ModelState.AddModelError(string.Empty, result.Message ?? "Ошибка при регистрации.");
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(result.Data!.Id);
            if (user != null)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
            }

            _logger.LogInformation("New user registered successfully: {Email}", model.Email);
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Отображает страницу входа. Если пользователь уже аутентифицирован, перенаправляет на главную.
        /// </summary>
        /// <param name="returnUrl">URL для возврата после успешного входа (необязательно).</param>
        /// <returns>Представление входа или перенаправление.</returns>
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated ?? false)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Обрабатывает вход пользователя по email и паролю.
        /// </summary>
        /// <param name="model">Учетные данные из формы.</param>
        /// <param name="returnUrl">URL для возврата после успешного входа (необязательно).</param>
        /// <returns>Перенаправление при успехе или представление с ошибкой.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, isPersistent: true, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in successfully.", model.Email);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            // Неудачная попытка входа логируется с уровнем Error, т.к. может указывать
            // на попытку подбора учетных данных.
            _logger.LogError("Failed login attempt for email {Email}. Invalid credentials.", model.Email);

            ModelState.AddModelError(string.Empty, "Неверный адрес электронной почты или пароль.");
            return View(model);
        }

        /// <summary>
        /// Отображает страницу смены пароля. Доступно только аутентифицированным пользователям.
        /// </summary>
        /// <returns>Представление смены пароля.</returns>
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// Обрабатывает смену пароля текущего пользователя.
        /// </summary>
        /// <param name="model">Данные смены пароля из формы.</param>
        /// <returns>Перенаправление при успехе или представление с ошибками.</returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(
                user, model.CurrentPassword, model.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    if (error.Code == "PasswordMismatch")
                    {
                        // Неверный текущий пароль — это ошибка пользователя, а не сбой системы,
                        // поэтому логируется как Warning.
                        _logger.LogWarning("Failed password change attempt for user {Email}. Incorrect current password.", user.Email);
                        ModelState.AddModelError(string.Empty, "Неверный текущий пароль");
                    }
                    else
                    {
                        _logger.LogWarning("Failed password change for user {Email}. Error: {Error}", user.Email, error.Description);
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User {Email} successfully changed their password.", user.Email);

            TempData["SuccessMessage"] = "Ваш пароль был успешно изменен.";
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Выполняет выход текущего пользователя из системы.
        /// </summary>
        /// <returns>Перенаправление на главную страницу.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userEmail = User.Identity?.Name;
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User {Email} logged out.", userEmail);
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Отображает страницу с сообщением об отказе в доступе.
        /// </summary>
        /// <returns>Представление с сообщением об отказе в доступе и HTTP-код 403.</returns>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            Response.StatusCode = 403;
            return View();
        }
    }
}
