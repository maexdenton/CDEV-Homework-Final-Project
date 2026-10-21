using System.Collections.Generic;
using System.Threading.Tasks;
using Blog.DAL.Entities;

namespace Blog.DAL.Repositories
{
    /// <summary>
    /// Контракт репозитория для работы с сущностями «Статья».
    /// </summary>
    public interface IArticleRepository : IRepository<Article>
    {
        new Task<IEnumerable<Article>> GetAllAsync();

        new Task<Article?> GetByIdAsync(int id);

        Task<Article?> GetDetailedByIdAsync(int id);

        Task<IEnumerable<Article>> GetFilteredAsync(string? search, string? tag);

        Task<IEnumerable<Article>> GetByAuthorIdAsync(string authorId);
    }
}
