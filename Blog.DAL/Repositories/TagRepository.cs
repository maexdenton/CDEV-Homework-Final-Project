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

        public TagRepository(BlogDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tag>> GetAllAsync() =>
            await _context.Tags.Include(t => t.Articles).Include(t => t.Creator).AsNoTracking().ToListAsync();

        public async Task<Tag?> GetByIdAsync(int id) =>
            await _context.Tags.Include(t => t.Articles).Include(t => t.Creator).FirstOrDefaultAsync(t => t.Id == id);

        public async Task<Tag?> GetByNameAsync(string name) =>
            await _context.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());

        public async Task<List<Tag>> GetOrCreateTagsAsync(IEnumerable<string> tagNames, string? creatorId = null)
        {
            var cleanNames = tagNames
                .Select(t => t.Trim().ToLower())
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct()
                .ToList();

            var existing = await _context.Tags.Where(t => cleanNames.Contains(t.Name.ToLower())).ToListAsync();
            var missingNames = cleanNames.Except(existing.Select(e => e.Name.ToLower())).ToList();

            var createdTags = new List<Tag>();
            foreach (var name in missingNames)
            {
                var tag = new Tag { Name = name, CreatorId = creatorId };
                createdTags.Add(tag);
            }

            if (createdTags.Any())
            {
                await _context.Tags.AddRangeAsync(createdTags);
                await _context.SaveChangesAsync();
            }

            return existing.Concat(createdTags).ToList();
        }

        public async Task AddAsync(Tag entity) => await _context.Tags.AddAsync(entity);
        public void Update(Tag entity) => _context.Tags.Update(entity);
        public void Delete(Tag entity) => _context.Tags.Remove(entity);
        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
