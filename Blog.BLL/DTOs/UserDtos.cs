using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Blog.BLL.DTOs
{
    /// <summary>
    /// DTO для регистрации новой учетной записи
    /// </summary>
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "Имя обязательно для заполнения")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Имя должно содержать от 2 до 50 символов")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилия обязательна для заполнения")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Фамилия должна содержать от 2 до 50 символов")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат адреса электронной почты")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть длиной не менее 6 символов")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO для редактирования профиля пользователя.
    /// Исключает прямое изменение пароля и прав через этот контракт
    /// </summary>
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "Имя обязательно для заполнения")]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилия обязательна для заполнения")]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Некорректный номер телефона")]
        public string? PhoneNumber { get; set; }
    }

    /// <summary>
    /// DTO для отображения информации о пользователе.
    /// Не содержит чувствительных данных (хэшей паролей, секретных ключей)
    /// </summary>
    public record UserDto(
        string Id,
        string Email,
        string FirstName,
        string LastName,
        string? PhoneNumber,
        DateTime RegisteredAt,
        IEnumerable<string> Roles
    );

    /// <summary>
    /// DTO для изменения роли
    /// </summary>
    public class ChangeUserRoleDto
    {
        [Required(ErrorMessage = "Имя роли обязательно")]
        public string RoleName { get; set; } = string.Empty;
    }
}
