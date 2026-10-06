using System;
using System.Collections.Generic;
using System.Text;
using Blog.DAL.Entities;

namespace Blog.DAL.Repositories
{
    /// <summary>
    /// Контракт репозитория для работы с публикациями блога
    /// </summary>
    public interface IArticleRepository : IRepository<Article>
    {
        /// <summary>
        /// Возвращает статьи с фильтрацией по ключевым словам и тегу
        /// </summary>
        Task<IEnumerable<Article>> GetFilteredAsync(string? search, string? tag);

        /// <summary>
        /// Возвращает статью по идентификатору с загрузкой автора, тегов и комментариев
        /// </summary>
        Task<Article?> GetDetailedByIdAsync(int id);

        /// <summary>
        /// Возвращает все статьи, опубликованные конкретным автором
        /// </summary>
        /// <param name="authorId">Идентификатор автора в Identity</param>
        Task<IEnumerable<Article>> GetByAuthorIdAsync(string authorId);
    }
}
