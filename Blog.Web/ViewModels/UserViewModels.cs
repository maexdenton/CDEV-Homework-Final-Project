using System.ComponentModel.DataAnnotations;

namespace Blog.Web.ViewModels
{
    /// <summary>
    /// Модель представления для отображения списка пользователей в таблице.
    /// Используется только для чтения, поэтому атрибуты валидации отсутствуют.
    /// Содержит минимальный набор полей для идентификации пользователя и его ролей.
    /// </summary>
    public class UserListViewModel
    {
        /// <summary>
        /// Идентификатор пользователя.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// Адрес электронной почты пользователя.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Фамилия пользователя.
        /// </summary>
        public string LastName { get; set; } = null!;

        /// <summary>
        /// Список ролей, в которых состоит пользователь.
        /// </summary>
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }

    /// <summary>
    /// Модель представления для страницы детального просмотра профиля пользователя.
    /// Дополнена полями, которые не нужны в списке: телефон и дата регистрации.
    /// </summary>
    public class UserDetailsViewModel
    {
        /// <summary>
        /// Идентификатор пользователя.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// Адрес электронной почты пользователя.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Фамилия пользователя.
        /// </summary>
        public string LastName { get; set; } = null!;

        /// <summary>
        /// Номер телефона пользователя. Необязательное поле.
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Дата регистрации пользователя. Отображается в профиле
        /// и может использоваться для сортировки.
        /// </summary>
        public DateTime RegisteredAt { get; set; }

        /// <summary>
        /// Список ролей, в которых состоит пользователь.
        /// </summary>
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }

    /// <summary>
    /// Модель представления для формы создания нового пользователя.
    /// Содержит поля, которые заполняет администратор: профиль, учетные данные и роли.
    /// </summary>
    public class UserCreateViewModel
    {
        /// <summary>
        /// Имя пользователя (от 2 до 50 символов).
        /// </summary>
        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Фамилия пользователя (от 2 до 50 символов).
        /// </summary>
        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = null!;

        /// <summary>
        /// Адрес электронной почты (логин).
        /// </summary>
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный Email")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Пароль пользователя (минимум 6 символов).
        /// </summary>
        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = null!;

        /// <summary>
        /// Подтверждение пароля. Должно совпадать с <see cref="Password"/>.
        /// Проверка через <see cref="CompareAttribute"/> защищает от опечатки
        /// при вводе пароля администратором.
        /// </summary>
        [Required(ErrorMessage = "Подтверждение пароля обязательно")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = null!;

        /// <summary>
        /// Роли, выбранные администратором при создании пользователя (чекбоксы).
        /// </summary>
        public List<string> SelectedRoles { get; set; } = new List<string>();

        /// <summary>
        /// Все доступные роли для отрисовки чекбоксов.
        /// </summary>
        public IEnumerable<string> AllRoles { get; set; } = new List<string>();
    }

    /// <summary>
    /// Модель представления для формы редактирования пользователя.
    /// В отличие от создания, здесь нет полей пароля: смена пароля — отдельный сценарий.
    /// </summary>
    public class UserEditViewModel
    {
        /// <summary>
        /// Идентификатор редактируемого пользователя.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// Имя пользователя (от 2 до 50 символов).
        /// </summary>
        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Фамилия пользователя (от 2 до 50 символов).
        /// </summary>
        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = null!;

        /// <summary>
        /// Адрес электронной почты (логин).
        /// </summary>
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный Email")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Текущие роли пользователя, выбранные для редактирования.
        /// </summary>
        public List<string> SelectedRoles { get; set; } = new List<string>();

        /// <summary>
        /// Все доступные роли для отрисовки чекбоксов.
        /// </summary>
        public IEnumerable<string> AllRoles { get; set; } = new List<string>();
    }

    /// <summary>
    /// Модель представления для страницы подтверждения удаления пользователя.
    /// </summary>
    public class UserDeleteViewModel
    {
        /// <summary>
        /// Идентификатор удаляемого пользователя.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// Адрес электронной почты удаляемого пользователя (для отображения в подтверждении).
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Имя удаляемого пользователя (для отображения в подтверждении).
        /// </summary>
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Фамилия удаляемого пользователя (для отображения в подтверждении).
        /// </summary>
        public string LastName { get; set; } = null!;
    }
}
