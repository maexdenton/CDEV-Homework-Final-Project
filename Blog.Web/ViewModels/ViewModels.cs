using System.ComponentModel.DataAnnotations;
using Blog.BLL.DTOs;

namespace Blog.Web.ViewModels
{
    public class ArticleIndexViewModel
    {
        public IEnumerable<ArticleDto> Articles { get; set; } = new List<ArticleDto>();
        public string? SearchQuery { get; set; }
        public string? TagFilter { get; set; }
    }

    public class ArticleCreateViewModel
    {
        [Required(ErrorMessage = "Заголовок обязателен")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Длина от 5 до 200 знаков")]
        [Display(Name = "Заголовок")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Текст статьи обязателен")]
        [Display(Name = "Текст публикации")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Теги (через запятую)")]
        public string Tags { get; set; } = string.Empty;
    }

    public class ArticleDetailsPageViewModel
    {
        public ArticleDetailsDto Article { get; set; } = null!;

        [Required(ErrorMessage = "Текст комментария не может быть пустым")]
        public string NewCommentContent { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
