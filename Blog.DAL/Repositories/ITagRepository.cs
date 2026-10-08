using Blog.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.DAL.Repositories
{
    /// <summary>
    /// Контракт репозитория тегов с поддержкой привязки создателя
    /// </summary>
    public interface ITagRepository : IRepository<Tag>
    {
        Task<Tag?> GetByNameAsync(string name);

        /// <summary>
        /// Разрешает список тегов: находит существующие и создает отсутствующие с фиксацией автора
        /// </summary>
        Task<List<Tag>> GetOrCreateTagsAsync(IEnumerable<string> tagNames, string? creatorId = null);
    }
}
