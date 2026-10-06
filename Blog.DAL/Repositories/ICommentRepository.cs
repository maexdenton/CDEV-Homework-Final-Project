using Blog.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.DAL.Repositories
{
    /// <summary>
    /// Контракт репозитория для работы с комментариями
    /// </summary>
    public interface ICommentRepository : IRepository<Comment>
    {
        /// <summary>
        /// Возвращает все комментарии с подгруженными данными об авторах и статьях
        /// </summary>
        Task<IEnumerable<Comment>> GetAllWithDetailsAsync();

        /// <summary>
        /// Возвращает комментарий по идентификатору с загрузкой автора
        /// </summary>
        Task<Comment?> GetWithDetailsByIdAsync(int id);
    }
}
