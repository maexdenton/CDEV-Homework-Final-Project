using System.ComponentModel.DataAnnotations;

namespace Blog.Web.ViewModels
{
    /// <summary>
    /// Модель представления для формы регистрации нового пользователя.
    /// Содержит поля профиля (имя, фамилия) и учетные данные (email, пароль).
    /// Подтверждение пароля проверяется атрибутом <see cref="CompareAttribute"/>,
    /// чтобы исключить опечатку при вводе.
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// Имя пользователя.
        /// </summary>
        [Required(ErrorMessage = "Имя обязательно для заполнения")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Фамилия пользователя.
        /// </summary>
        [Required(ErrorMessage = "Фамилия обязательна для заполнения")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Адрес электронной почты, используется как логин.
        /// </summary>
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат адреса электронной почты")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Пароль пользователя (минимум 6 символов).
        /// </summary>
        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть длиной не менее 6 символов")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Подтверждение пароля. Должно совпадать с <see cref="Password"/>.
        /// Имеет те же ограничения длины и типа данных, что и пароль,
        /// но дополнительно проверяется атрибутом <see cref="CompareAttribute"/>.
        /// </summary>
        [Required(ErrorMessage = "Подтверждение пароля обязательно")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть длиной не менее 6 символов")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        [Display(Name = "Подтверждение пароля")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Модель представления для формы входа.
    /// Email используется как логин и обязателен для заполнения.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Адрес электронной почты (логин).
        /// </summary>
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат адреса электронной почты")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Пароль пользователя.
        /// </summary>
        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Модель представления для смены пароля авторизованным пользователем.
    /// Текущий пароль подтверждает личность перед изменением.
    /// </summary>
    public class ChangePasswordViewModel
    {
        /// <summary>
        /// Текущий пароль пользователя для подтверждения личности.
        /// </summary>
        [Required(ErrorMessage = "Введите текущий пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string CurrentPassword { get; set; } = null!;

        /// <summary>
        /// Новый пароль (минимум 6 символов).
        /// </summary>
        [Required(ErrorMessage = "Введите новый пароль")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть длиной не менее 6 символов")]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; } = null!;

        /// <summary>
        /// Подтверждение нового пароля. Должно совпадать с <see cref="NewPassword"/>.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение нового пароля")]
        [Compare("NewPassword", ErrorMessage = "Новый пароль и его подтверждение не совпадают")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
