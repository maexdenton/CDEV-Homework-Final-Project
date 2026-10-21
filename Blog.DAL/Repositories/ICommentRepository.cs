using System.Collections.Generic;
using System.Threading.Tasks;
using Blog.DAL.Entities;

namespace Blog.DAL.Repositories
{
    /// <summary>
    /// Контракт репозитория для работы с сущностями «Комментарий».
    /// </summary>
    public interface ICommentRepository : IRepository<Comment>
    {
        new Task<IEnumerable<Comment>> GetAllAsync();

        new Task<Comment?> GetByIdAsync(int id);

        Task<IEnumerable<Comment>> GetAllWithDetailsAsync();

        Task<Comment?> GetWithDetailsByIdAsync(int id);
    }
}
