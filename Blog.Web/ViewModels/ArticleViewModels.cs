using System.ComponentModel.DataAnnotations;
using Blog.BLL.DTOs;

namespace Blog.Web.ViewModels
{
    // Модель представления для формы редактирования статьи.
    // Содержит поля, которые пользователь может изменить (заголовок, текст, теги), 
    // и правила валидации для них. Идентификатор статьи передается скрытым полем.
    public class ArticleEditViewModel
    {
        // Идентификатор редактируемой статьи
        public int Id { get; set; }

        [Required(ErrorMessage = "Заголовок обязателен для заполнения")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Заголовок должен содержать от 5 до 200 символов")]
        [Display(Name = "Заголовок статьи")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Текст публикации обязателен для заполнения")]
        [MinLength(10, ErrorMessage = "Текст статьи должен содержать не менее 10 символов")]
        [Display(Name = "Текст публикации")]
        public string Content { get; set; } = string.Empty;

        // Теги вводятся одной строкой через запятую, контроллер разбивает их на список
        [Display(Name = "Теги (через запятую)")]
        public string Tags { get; set; } = string.Empty;
    }
}
