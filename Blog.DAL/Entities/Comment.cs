using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.DAL.Entities
{
    // Сущность "Комментарий". Представляет отзыв, оставленный пользователем под конкретной статьей.
    // Правила валидации и связи с таблицами настраиваются в CommentConfiguration (Fluent API).
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int ArticleId { get; set; }
        public Article Article { get; set; } = null!;

        public string AuthorId { get; set; } = string.Empty;
        public User Author { get; set; } = null!;
    }
}
