using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Blog.DAL.Entities;

namespace Blog.DAL.Repositories
{
    /// <summary>
    /// Базовый generic-интерфейс репозитория.
    /// Обеспечивает стандартные CRUD-операции для любой сущности.
    /// </summary>
    /// <typeparam name="T">Тип сущности, для которой предоставляются CRUD-операции.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Возвращает все сущности указанного типа.
        /// </summary>
        /// <returns>Коллекция сущностей.</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Возвращает сущность по ее идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор сущности.</param>
        /// <returns>Сущность или null, если не найдена.</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Добавляет новую сущность в контекст (без сохранения).
        /// </summary>
        /// <param name="entity">Сущность для добавления.</param>
        Task AddAsync(T entity);

        /// <summary>
        /// Помечает сущность как измененную.
        /// </summary>
        /// <param name="entity">Измененная сущность.</param>
        void Update(T entity);

        /// <summary>
        /// Помечает сущность как удаленную.
        /// </summary>
        /// <param name="entity">Сущность для удаления.</param>
        void Delete(T entity);

        /// <summary>
        /// Сохраняет все изменения в базе данных.
        /// </summary>
        Task SaveChangesAsync();
    }
}
