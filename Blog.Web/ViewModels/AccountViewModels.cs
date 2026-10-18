using System.ComponentModel.DataAnnotations;

namespace Blog.Web.ViewModels
{
    // Модель для формы регистрации
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Имя обязательно для заполнения")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилия обязательна для заполнения")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат адреса электронной почты")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть длиной не менее 6 символов")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        // Идентичный тип, ограничения длины и валидация, дополненные сравнения
        [Required(ErrorMessage = "Подтверждение пароля обязательно")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть длиной не менее 6 символов")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        [Display(Name = "Подтверждение пароля")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    // Модель для формы входа
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат адреса электронной почты")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    // Модель для формы смены пароля
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Введите текущий пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "Введите новый пароль")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть длиной не менее 6 символов")]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение нового пароля")]
        // Проверка совпадения с новым паролем
        [Compare("NewPassword", ErrorMessage = "Новый пароль и его подтверждение не совпадают")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
