using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.DAL.Entities
{
    // Сущность "Тег" (метки). Используется для категоризации статей 
    // и предоставления пользователям возможности фильтровать контент по ключевым словам.
    // Связь "многие-ко-многим" со статьями настраивается в TagConfiguration (Fluent API).
    public class Tag
    {
        public int Id { get; set; }

        // Уникальное текстовое имя тега
        public string Name { get; set; } = string.Empty;

        // Идентификатор пользователя, создавшего данный тег.
        // Позволяет проверять владение тегом при редактировании и удалении
        public string? CreatorId { get; set; }

        // Навигационное свойство на создателя тега
        public User? Creator { get; set; }

        // Коллекция статей, отмеченных данным тегом
        public ICollection<Article> Articles { get; set; } = new List<Article>();
    }
}
