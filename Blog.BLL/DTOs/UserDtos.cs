using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Blog.BLL.DTOs
{
    /// <summary>
    /// DTO для регистрации нового пользователя.
    /// </summary>
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 100 символов")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Подтверждение пароля обязательно")]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO для аутентификации пользователя.
    /// </summary>
    public class LoginDto
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// DTO для редактирования профиля пользователя.
    /// </summary>
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Некорректный номер телефона")]
        public string? PhoneNumber { get; set; }
    }

    /// <summary>
    /// DTO для смены пароля пользователя.
    /// </summary>
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Текущий пароль обязателен")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Новый пароль обязателен")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 100 символов")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Подтверждение пароля обязательно")]
        [Compare(nameof(NewPassword), ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO для изменения роли пользователя администратором.
    /// </summary>
    public class ChangeUserRoleDto
    {
        [Required(ErrorMessage = "Роль обязательна")]
        public string RoleName { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO для отображения данных пользователя.
    /// </summary>
    public record UserDto(
        string Id,
        string Email,
        string FirstName,
        string LastName,
        string? PhoneNumber,
        DateTime RegisteredAt,
        List<string> Roles
    );
}
