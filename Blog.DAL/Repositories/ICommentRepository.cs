using Blog.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.DAL.Repositories
{
    // Интерфейс репозитория для управления сущностями "Комментарий"
    public interface ICommentRepository : IRepository<Comment>
    {
        // Возвращает все комментарии с подгруженными данными об авторах и статьях
        Task<IEnumerable<Comment>> GetAllWithDetailsAsync();

        // Возвращает комментарий по идентификатору с загрузкой автора
        Task<Comment?> GetWithDetailsByIdAsync(int id);
    }
}
