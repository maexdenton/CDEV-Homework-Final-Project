using Blog.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Blog.DAL.Repositories
{
    /// <summary>
    /// Репозиторий для работы с сущностями «Тег».
    /// Реализует контракт <see cref="ITagRepository"/>, инкапсулируя запросы к БД,
    /// связанные с тегами и их связями со статьями и создателями.
    /// </summary>
    public class TagRepository : ITagRepository
    {
        private readonly BlogDbContext _context;

        public TagRepository(BlogDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Возвращает все теги с загруженными статьями и создателями (без отслеживания изменений).
        /// </summary>
        /// <returns>Коллекция тегов.</returns>
        public async Task<IEnumerable<Tag>> GetAllAsync() =>
            await _context.Tags
                .Include(t => t.Articles)
                .Include(t => t.Creator)
                .AsNoTracking()
                .ToListAsync();

        /// <summary>
        /// Возвращает тег по идентификатору с загруженными статьями и создателем.
        /// </summary>
        /// <param name="id">Идентификатор тега.</param>
        /// <returns>Тег или null, если не найден.</returns>
        public async Task<Tag?> GetByIdAsync(int id) =>
            await _context.Tags
                .Include(t => t.Articles)
                .Include(t => t.Creator)
                .FirstOrDefaultAsync(t => t.Id == id);

        /// <summary>
        /// Находит тег по точному названию (без учета регистра).
        /// </summary>
        /// <param name="name">Название тега.</param>
        /// <returns>Тег или null, если тег с таким именем не найден.</returns>
        public async Task<Tag?> GetByNameAsync(string name) =>
            await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());

        /// <summary>
        /// Разрешает список имен тегов: возвращает существующие теги и создает отсутствующие
        /// с указанием создателя. Имена нормализуются (Trim, ToLower) и дедуплицируются.
        /// </summary>
        /// <param name="tagNames">Коллекция имен тегов.</param>
        /// <param name="creatorId">Идентификатор создателя для новых тегов (необязательно).</param>
        /// <returns>Коллекция сущностей тегов, соответствующих переданным именам.</returns>
        public async Task<IEnumerable<Tag>> GetOrCreateTagsAsync(IEnumerable<string> tagNames, string? creatorId = null)
        {
            var cleanNames = tagNames
                .Select(t => t.Trim().ToLower())
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct()
                .ToList();

            var existing = await _context.Tags
                .Where(t => cleanNames.Contains(t.Name.ToLower()))
                .ToListAsync();

            var missingNames = cleanNames
                .Except(existing.Select(e => e.Name.ToLower()))
                .ToList();

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

        /// <summary>
        /// Добавляет новый тег в контекст (без сохранения).
        /// </summary>
        /// <param name="entity">Сущность тега.</param>
        public async Task AddAsync(Tag entity) =>
            await _context.Tags.AddAsync(entity);

        /// <summary>
        /// Помечает тег как измененный (например, при переименовании).
        /// </summary>
        /// <param name="entity">Измененная сущность тега.</param>
        public void Update(Tag entity) =>
            _context.Tags.Update(entity);

        /// <summary>
        /// Помечает тег как удаленный.
        /// </summary>
        /// <param name="entity">Сущность тега для удаления.</param>
        public void Delete(Tag entity) =>
            _context.Tags.Remove(entity);

        /// <summary>
        /// Сохраняет все накопленные изменения в базе данных.
        /// </summary>
        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}
