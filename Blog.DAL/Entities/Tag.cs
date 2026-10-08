using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.DAL.Entities
{
    /// <summary>
    /// Сущность тега (метки), привязываемой к публикациям
    /// </summary>
    public class Tag
    {
        public int Id { get; set; }

        /// <summary>
        /// Уникальное текстовое имя тега
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Идентификатор пользователя, создавшего данный тег.
        /// Позволяет проверять владение тегом при редактировании и удалении
        /// </summary>
        public string? CreatorId { get; set; }

        /// <summary>
        /// Навигационное свойство на создателя тега
        /// </summary>
        public User? Creator { get; set; }

        /// <summary>
        /// Коллекция статей, отмеченных данным тегом
        /// </summary>
        public ICollection<Article> Articles { get; set; } = new List<Article>();
    }
}
