using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace Blog.DAL.Entities
{
    /// <summary>
    /// Сущность «Пользователь». Расширяет стандартный <see cref="IdentityUser"/> дополнительными полями профиля
    /// (имя, фамилия, дата регистрации) и навигационными свойствами для связей с контентом блога.
    /// </summary>
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public ICollection<Article> Articles { get; set; } = new List<Article>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
