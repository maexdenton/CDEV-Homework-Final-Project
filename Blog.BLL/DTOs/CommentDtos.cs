using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Blog.BLL.DTOs
{
    // DTO добавления комментария к публикации
    public class CreateCommentDto
    {
        [Required(ErrorMessage = "Идентификатор статьи обязателен")]
        public int ArticleId { get; set; }

        [Required(ErrorMessage = "Идентификатор автора обязателен")]
        public string AuthorId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Текст комментария не может быть пустым")]
        [StringLength(1000, MinimumLength = 2, ErrorMessage = "Длина комментария от 2 до 1000 знаков")]
        public string Content { get; set; } = string.Empty;
    }

    // DTO редактирования существующего комментария
    public class UpdateCommentDto
    {
        [Required(ErrorMessage = "Текст комментария не может быть пустым")]
        [StringLength(1000, MinimumLength = 2, ErrorMessage = "Длина комментария от 2 до 1000 знаков")]
        public string Content { get; set; } = string.Empty;
    }

    // DTO отображения комментария
    public record CommentDto(
        int Id,
        int ArticleId,
        string Content,
        DateTime CreatedAt,
        string AuthorId,
        string AuthorFullName
    );
}
