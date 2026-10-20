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
    /// MVC-контроллер для аутентификации и управления учетной записью
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        // Логгер для фиксации событий безопасности и ошибок входа
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

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated ?? false)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: /Account/Register
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
                // Ошибки бизнес-логики (например, email уже занят) логируем как Warning
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

        // GET: /Account/Login
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

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                isPersistent: true,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in successfully.", model.Email);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            // ЛОГИРОВАНИЕ ОШИБКИ АВТОРИЗАЦИИ (согласно требованию - уровень Error)
            _logger.LogError("Failed login attempt for email {Email}. Invalid credentials.", model.Email);

            ModelState.AddModelError(string.Empty, "Неверный адрес электронной почты или пароль.");
            return View(model);
        }

        // GET: /Account/ChangePassword
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
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    if (error.Code == "PasswordMismatch")
                    {
                        // Неверный текущий пароль - логируем как Warning (ошибка пользователя)
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

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userEmail = User.Identity?.Name;
            await _signInManager.SignOutAsync();

            _logger.LogInformation("User {Email} logged out.", userEmail);
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
