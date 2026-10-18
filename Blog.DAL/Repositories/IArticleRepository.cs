using System;
using System.Collections.Generic;
using System.Text;
using Blog.DAL.Entities;

namespace Blog.DAL.Repositories
{
    // Интерфейс репозитория для управления сущностями "Статья"
    public interface IArticleRepository : IRepository<Article>
    {
        // Возвращает статьи с фильтрацией по ключевым словам и тегу
        Task<IEnumerable<Article>> GetFilteredAsync(string? search, string? tag);

        // Возвращает статью по идентификатору с загрузкой автора, тегов и комментариев
        Task<Article?> GetDetailedByIdAsync(int id);


        /// <summary>
        /// Возвращает все статьи, опубликованные конкретным автором
        /// </summary>
        /// <param name="authorId">Идентификатор автора в Identity</param>
        Task<IEnumerable<Article>> GetByAuthorIdAsync(string authorId);
    }
}
