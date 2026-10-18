using System.ComponentModel.DataAnnotations;

namespace Blog.Web.ViewModels
{
    // Модель для отображения списка пользователей в таблице.
    // Используется только для чтения, поэтому атрибуты валидации отсутствуют.
    // Содержит минимальный набор полей, необходимых для идентификации пользователя и его ролей.
    public class UserListViewModel
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        // Список ролей, в которых состоит пользователь
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }

    // Модель для страницы детального просмотра профиля пользователя.
    // Дополнена полями, которые не нужны в списке: телефон и дата регистрации.
    public class UserDetailsViewModel
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        
        // Телефон необязателен
        public string? PhoneNumber { get; set; }

        // Дата регистрации: отображается в профиле и может использоваться для сортировки
        public DateTime RegisteredAt { get; set; }
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }

    // Модель для формы создания нового пользователя.
    // Содержит поля, которые заполняет администратор: профиль, учетные данные и роли.
    public class UserCreateViewModel
    {
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

        // Подтверждение пароля проверяется атрибутом [Compare].
        // Это защищает от опечатки при вводе пароля администратором. 
        [Required(ErrorMessage = "Подтверждение пароля обязательно")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = null!;

        // Выбранные роли при создании (чекбоксы)
        public List<string> SelectedRoles { get; set; } = new List<string>();

        // Все доступные роли для отрисовки чекбоксов
        public IEnumerable<string> AllRoles { get; set; } = new List<string>();
    }

    // Модель для формы редактирования пользователя.
    // В отличие от создания, здесь нет полей пароля (смена пароля - отдельный сценарий).
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

    // Модель страницы подтверждения удаления пользователя 
    public class UserDeleteViewModel
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
    }
}
