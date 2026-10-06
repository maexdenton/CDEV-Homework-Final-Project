using Blog.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.DAL.Repositories
{
    public interface ITagRepository : IRepository<Tag>
    {
        Task<Tag?> GetByNameAsync(string name);
        Task<List<Tag>> GetOrCreateTagsAsync(IEnumerable<string> tagNames);
    }
}
