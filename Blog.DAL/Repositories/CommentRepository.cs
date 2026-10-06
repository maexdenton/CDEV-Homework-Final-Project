using Blog.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Blog.DAL.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly BlogDbContext _context;
        public CommentRepository(BlogDbContext context) => _context = context;

        public async Task<IEnumerable<Comment>> GetAllAsync() => await _context.Comments.ToListAsync();
        public async Task<Comment?> GetByIdAsync(int id) => await _context.Comments.FindAsync(id);
        public async Task AddAsync(Comment entity) => await _context.Comments.AddAsync(entity);
        public void Update(Comment entity) => _context.Comments.Update(entity);
        public void Delete(Comment entity) => _context.Comments.Remove(entity);
        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
