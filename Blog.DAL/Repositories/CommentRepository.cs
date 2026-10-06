using Blog.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Blog.DAL.Repositories
{
    /// <summary>
    /// Репозиторий доступа к данным комментариев
    /// </summary>
    public class CommentRepository : ICommentRepository
    {
        private readonly BlogDbContext _context;

        public CommentRepository(BlogDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> GetAllAsync() =>
            await _context.Comments.AsNoTracking().ToListAsync();

        public async Task<IEnumerable<Comment>> GetAllWithDetailsAsync() =>
            await _context.Comments
                .Include(c => c.Author)
                .Include(c => c.Article)
                .OrderByDescending(c => c.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

        public async Task<Comment?> GetByIdAsync(int id) =>
            await _context.Comments.FindAsync(id);

        public async Task<Comment?> GetWithDetailsByIdAsync(int id) =>
            await _context.Comments
                .Include(c => c.Author)
                .Include(c => c.Article)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task AddAsync(Comment entity) => await _context.Comments.AddAsync(entity);
        public void Update(Comment entity) => _context.Comments.Update(entity);
        public void Delete(Comment entity) => _context.Comments.Remove(entity);
        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
