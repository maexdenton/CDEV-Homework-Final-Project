using Blog.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Blog.DAL.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly BlogDbContext _context;
        public TagRepository(BlogDbContext context) => _context = context;

        public async Task<IEnumerable<Tag>> GetAllAsync() => await _context.Tags.ToListAsync();
        public async Task<Tag?> GetByIdAsync(int id) => await _context.Tags.FindAsync(id);
        public async Task<Tag?> GetByNameAsync(string name) =>
            await _context.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());

        public async Task<List<Tag>> GetOrCreateTagsAsync(IEnumerable<string> tagNames)
        {
            var cleanNames = tagNames.Select(t => t.Trim().ToLower()).Where(t => !string.IsNullOrEmpty(t)).Distinct().ToList();
            var existing = await _context.Tags.Where(t => cleanNames.Contains(t.Name.ToLower())).ToListAsync();
            var toCreate = cleanNames.Except(existing.Select(e => e.Name.ToLower())).Select(n => new Tag { Name = n }).ToList();

            if (toCreate.Any())
            {
                await _context.Tags.AddRangeAsync(toCreate);
                await _context.SaveChangesAsync();
            }
            return existing.Concat(toCreate).ToList();
        }

        public async Task AddAsync(Tag entity) => await _context.Tags.AddAsync(entity);
        public void Update(Tag entity) => _context.Tags.Update(entity);
        public void Delete(Tag entity) => _context.Tags.Remove(entity);
        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
