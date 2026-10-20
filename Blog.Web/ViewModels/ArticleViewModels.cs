using System.ComponentModel.DataAnnotations;

namespace Blog.Web.ViewModels
{
    /// <summary>
    /// Модель представления для формы создания статьи.
    /// Содержит заголовок, текст и строку тегов, разделенных запятыми.
    /// </summary>
    public class ArticleCreateViewModel
    {
        /// <summary>
        /// Заголовок статьи (от 5 до 200 символов).
        /// </summary>
        [Required(ErrorMessage = "Заголовок обязателен для заполнения")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Заголовок должен содержать от 5 до 200 символов")]
        [Display(Name = "Заголовок статьи")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Текст публикации (не менее 10 символов).
        /// </summary>
        [Required(ErrorMessage = "Текст публикации обязателен для заполнения")]
        [MinLength(10, ErrorMessage = "Текст статьи должен содержать не менее 10 символов")]
        [Display(Name = "Текст публикации")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Теги в виде строки, разделенной запятыми.
        /// При сохранении разбивается на отдельные теги в контроллере.
        /// </summary>
        [Display(Name = "Теги (через запятую)")]
        public string Tags { get; set; } = string.Empty;
    }

    /// <summary>
    /// Модель представления для формы редактирования статьи.
    /// Содержит те же поля, что и <see cref="ArticleCreateViewModel"/>,
    /// плюс идентификатор редактируемой статьи.
    /// </summary>
    public class ArticleEditViewModel
    {
        /// <summary>
        /// Идентификатор редактируемой статьи.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Заголовок статьи (от 5 до 200 символов).
        /// </summary>
        [Required(ErrorMessage = "Заголовок обязателен для заполнения")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Заголовок должен содержать от 5 до 200 символов")]
        [Display(Name = "Заголовок статьи")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Текст публикации (не менее 10 символов).
        /// </summary>
        [Required(ErrorMessage = "Текст публикации обязателен для заполнения")]
        [MinLength(10, ErrorMessage = "Текст статьи должен содержать не менее 10 символов")]
        [Display(Name = "Текст публикации")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Теги в виде строки, разделенной запятыми.
        /// При сохранении разбивается на отдельные теги в контроллере.
        /// </summary>
        [Display(Name = "Теги (через запятую)")]
        public string Tags { get; set; } = string.Empty;
    }
}
