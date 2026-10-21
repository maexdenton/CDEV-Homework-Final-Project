using System.ComponentModel.DataAnnotations;

namespace Blog.Web.ViewModels
{
    /// <summary>
    /// Модель представления для списка ролей и страницы деталей.
    /// Используется только для чтения, поэтому атрибуты валидации отсутствуют.
    /// </summary>
    public class RoleListViewModel
    {
        /// <summary>
        /// Идентификатор роли. Необходим для формирования ссылок
        /// на действия <c>Edit</c>, <c>Delete</c> и <c>Details</c>.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// Название роли.
        /// </summary>
        public string Name { get; set; } = null!;
    }

    /// <summary>
    /// Модель представления для создания роли.
    /// Идентификатор генерируется автоматически, поэтому в модели отсутствует.
    /// </summary>
    public class RoleCreateViewModel
    {
        /// <summary>
        /// Название роли (от 2 до 50 символов).
        /// </summary>
        [Required(ErrorMessage = "Название роли обязательно")]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = null!;
    }

    /// <summary>
    /// Модель представления для редактирования роли.
    /// Содержит идентификатор для выбора записи и название с теми же правилами валидации,
    /// что и при создании.
    /// </summary>
    public class RoleEditViewModel
    {
        /// <summary>
        /// Идентификатор редактируемой роли.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// Название роли (от 2 до 50 символов).
        /// </summary>
        [Required(ErrorMessage = "Название роли обязательно")]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = null!;
    }

    /// <summary>
    /// Модель представления для подтверждения удаления роли.
    /// <see cref="Id"/> используется для выполнения запроса, <see cref="Name"/> — для отображения пользователю.
    /// </summary>
    public class RoleDeleteViewModel
    {
        /// <summary>
        /// Идентификатор удаляемой роли.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// Название удаляемой роли (для отображения в форме подтверждения).
        /// </summary>
        public string Name { get; set; } = null!;
    }
}
