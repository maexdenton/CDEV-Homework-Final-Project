using System;
using System.Collections.Generic;
using System.Text;
using Blog.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blog.DAL.Repositories
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly BlogDbContext _context;

        public ArticleRepository(BlogDbContext context) => _context = context;

        public async Task<IEnumerable<Article>> GetAllAsync() =>
            await _context.Articles.Include(a => a.Author).Include(a => a.Tags).AsNoTracking().ToListAsync();

        public async Task<Article?> GetByIdAsync(int id) =>
            await _context.Articles.FindAsync(id);

        public async Task<Article?> GetDetailedByIdAsync(int id) =>
            await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Tags)
                .Include(a => a.Comments).ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(a => a.Id == id);

        public async Task<IEnumerable<Article>> GetFilteredAsync(string? search, string? tag)
        {
            var query = _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Tags)
                .Include(a => a.Comments)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var cleanSearch = search.Trim().ToLower();
                query = query.Where(a => a.Title.ToLower().Contains(cleanSearch) || a.Content.ToLower().Contains(cleanSearch));
            }

            if (!string.IsNullOrWhiteSpace(tag))
            {
                var cleanTag = tag.Trim().ToLower();
                query = query.Where(a => a.Tags.Any(t => t.Name.ToLower() == cleanTag));
            }

            return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        }

        public async Task AddAsync(Article entity) => await _context.Articles.AddAsync(entity);
        public void Update(Article entity) => _context.Articles.Update(entity);
        public void Delete(Article entity) => _context.Articles.Remove(entity);
        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
