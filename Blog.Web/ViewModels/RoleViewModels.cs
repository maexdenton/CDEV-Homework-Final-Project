using System.ComponentModel.DataAnnotations;

namespace Blog.Web.ViewModels
{
    // Модель для отображения списка ролей и страницы деталей. 
    // Используется только для чтения, поэтому атрибуты валидации отсутствуют. 
    // Свойство Id необходимо для формирования ссылок на действия Edit/Delete/Details.
    public class RoleListViewModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    // Модель создания роли: Id генерируется автоматически, название ограничено 50 символами
    public class RoleCreateViewModel
    {
        [Required(ErrorMessage = "Название роли обязательно")]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = null!;
    }

    // Модель редактирования роли: Id идентифицирует запись, название валидируется как при создании
    public class RoleEditViewModel
    {
        public string Id { get; set; } = null!;

        [Required(ErrorMessage = "Название роли обязательно")]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = null!;
    }

    // Модель подтверждения удаления роли: Id для запроса, Name для отображения
    public class RoleDeleteViewModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
