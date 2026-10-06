using System;
using System.Collections.Generic;
using System.Text;
using Blog.DAL.Entities;

namespace Blog.DAL.Repositories
{
    public interface IArticleRepository : IRepository<Article>
    {
        Task<IEnumerable<Article>> GetFilteredAsync(string? search, string? tag);
        Task<Article?> GetDetailedByIdAsync(int id);
    }
}
