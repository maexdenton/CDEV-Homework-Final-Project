using Blog.BLL.DTOs;
using Blog.BLL.Services;
using Blog.DAL.Entities;
using Blog.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers
{
    /// <summary>
    /// Контроллер аутентификации и регистрации для веб-интерфейса (MVC)
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AccountController(
            IUserService userService,
            SignInManager<User> signInManager,
            UserManager<User> userManager)
        {
            _userService = userService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated ?? false)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Передаем регистрацию в BLL-сервис (там автоматически создается роль "Пользователь" и хэшируется пароль)
            var dto = new RegisterUserDto
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword
            };

            var (succeeded, error, userDto) = await _userService.RegisterAsync(dto);

            if (!succeeded)
            {
                ModelState.AddModelError(string.Empty, error ?? "Ошибка при регистрации.");
                return View(model);
            }

            // Автоматический вход после успешной регистрации
            var user = await _userManager.FindByIdAsync(userDto!.Id);
            if (user != null)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated ?? false)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

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
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Неверный адрес электронной почты или пароль.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
