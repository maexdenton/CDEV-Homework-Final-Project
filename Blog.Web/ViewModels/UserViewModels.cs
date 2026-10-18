using System.ComponentModel.DataAnnotations;

namespace Blog.Web.ViewModels
{
    // Модель для отображения списка пользователей (только необходимые поля)
    public class UserListViewModel
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        // Список ролей, в которых состоит пользователь
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }

    // Модель для детального просмотра профиля
    public class UserDetailsViewModel
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public DateTime RegisteredAt { get; set; }
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }

    // Модель для создания нового пользователя
    public class UserCreateViewModel
    {
        // Атрибуты валидации данных для серверной и клиентской проверки
        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = null!;

        // Проверка совпадения паролей через атрибут Compare
        [Required(ErrorMessage = "Подтверждение пароля обязательно")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = null!;

        // Выбранные роли при создании (чекбоксы)
        public List<string> SelectedRoles { get; set; } = new List<string>();
        // Все доступные роли для отрисовки чекбоксов
        public IEnumerable<string> AllRoles { get; set; } = new List<string>();
    }

    // Модель для редактирования пользователя
    public class UserEditViewModel
    {
        public string Id { get; set; } = null!;

        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный Email")]
        public string Email { get; set; } = null!;

        public List<string> SelectedRoles { get; set; } = new List<string>();
        public IEnumerable<string> AllRoles { get; set; } = new List<string>();
    }

    // Модель для подтверждения удаления
    public class UserDeleteViewModel
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
    }
}
