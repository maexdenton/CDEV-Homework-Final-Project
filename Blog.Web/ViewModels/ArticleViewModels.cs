using System.ComponentModel.DataAnnotations;
using Blog.BLL.DTOs;

namespace Blog.Web.ViewModels
{
    /// <summary>
    /// Модель представления для формы редактирования статьи
    /// </summary>
    public class ArticleEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Заголовок обязателен для заполнения")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Заголовок должен содержать от 5 до 200 символов")]
        [Display(Name = "Заголовок статьи")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Текст публикации обязателен для заполнения")]
        [MinLength(10, ErrorMessage = "Текст статьи должен содержать не менее 10 символов")]
        [Display(Name = "Текст публикации")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Теги (через запятую)")]
        public string Tags { get; set; } = string.Empty;
    }
}
