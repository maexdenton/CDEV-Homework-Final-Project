using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Blog.BLL.DTOs
{
    // DTO создания статьи
    public class CreateArticleDto
    {
        [Required(ErrorMessage = "Заголовок обязателен")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Заголовок должен быть от 5 до 200 символов")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Текст статьи обязателен")]
        [MinLength(10, ErrorMessage = "Текст статьи должен содержать не менее 10 символов")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Идентификатор автора обязателен")]
        public string AuthorId { get; set; } = string.Empty;

        public List<string> Tags { get; set; } = new();
    }

    // DTO редактирования статьи
    public class UpdateArticleDto
    {
        [Required(ErrorMessage = "Заголовок обязателен")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Заголовок должен быть от 5 до 200 символов")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Текст статьи обязателен")]
        [MinLength(10, ErrorMessage = "Текст статьи должен содержать не менее 10 символов")]
        public string Content { get; set; } = string.Empty;

        public List<string> Tags { get; set; } = new();
    }

    // DTO краткого отображения статьи для списков
    public record ArticleDto(
        int Id,
        string Title,
        string Content,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        string AuthorId,
        string AuthorFullName,
        List<string> Tags,
        int CommentsCount
    );

    // DTO подробного отображения статьи с комментариями
    public record ArticleDetailsDto(
        int Id,
        string Title,
        string Content,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        string AuthorId,
        string AuthorFullName,
        List<string> Tags,
        List<CommentDto> Comments
    );
}
