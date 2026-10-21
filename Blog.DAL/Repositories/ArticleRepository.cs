using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Blog.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blog.DAL.Repositories
{
    /// <summary>
    /// Репозиторий для работы с сущностями «Статья».
    /// Инкапсулирует все операции чтения и записи в БД для этой сущности.
    /// </summary>
    public class ArticleRepository : IArticleRepository
    {
        private readonly BlogDbContext _context;

        public ArticleRepository(BlogDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Возвращает все статьи с загруженными автором и тегами (без отслеживания изменений).
        /// </summary>
        /// <returns>Коллекция статей.</returns>
        public async Task<IEnumerable<Article>> GetAllAsync() =>
            await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Tags)
                .AsNoTracking()
                .ToListAsync();

        /// <summary>
        /// Возвращает статью по идентификатору без загрузки связанных данных.
        /// </summary>
        /// <param name="id">Идентификатор статьи.</param>
        /// <returns>Статья или null, если не найдена.</returns>
        public async Task<Article?> GetByIdAsync(int id) =>
            await _context.Articles.FindAsync(id);

        /// <summary>
        /// Возвращает статью с полной информацией: автор, теги и комментарии с авторами.
        /// </summary>
        /// <param name="id">Идентификатор статьи.</param>
        /// <returns>Статья с деталями или null, если не найдена.</returns>
        public async Task<Article?> GetDetailedByIdAsync(int id) =>
            await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Tags)
                .Include(a => a.Comments)
                    .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(a => a.Id == id);

        /// <summary>
        /// Возвращает статьи с необязательной фильтрацией по поисковой строке (заголовок/содержимое) и тегу.
        /// Результат сортируется по дате создания (сначала новые) и не отслеживается.
        /// </summary>
        /// <param name="search">Поисковая строка (необязательно).</param>
        /// <param name="tag">Название тега для фильтрации (необязательно).</param>
        /// <returns>Отфильтрованная коллекция статей.</returns>
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
                query = query.Where(a =>
                    a.Title.ToLower().Contains(cleanSearch) ||
                    a.Content.ToLower().Contains(cleanSearch));
            }

            if (!string.IsNullOrWhiteSpace(tag))
            {
                var cleanTag = tag.Trim().ToLower();
                query = query.Where(a => a.Tags.Any(t => t.Name.ToLower() == cleanTag));
            }

            return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        }

        /// <summary>
        /// Возвращает статьи указанного автора с загруженными тегами и комментариями.
        /// Сортировка по дате создания (сначала новые), без отслеживания изменений.
        /// </summary>
        /// <param name="authorId">Идентификатор автора.</param>
        /// <returns>Коллекция статей автора.</returns>
        public async Task<IEnumerable<Article>> GetByAuthorIdAsync(string authorId)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Tags)
                .Include(a => a.Comments)
                .Where(a => a.AuthorId == authorId)
                .OrderByDescending(a => a.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Добавляет новую статью в контекст (без сохранения).
        /// </summary>
        /// <param name="entity">Сущность статьи.</param>
        public async Task AddAsync(Article entity) =>
            await _context.Articles.AddAsync(entity);

        /// <summary>
        /// Помечает статью как измененную.
        /// </summary>
        /// <param name="entity">Сущность статьи.</param>
        public void Update(Article entity) =>
            _context.Articles.Update(entity);

        /// <summary>
        /// Помечает статью как удаленную.
        /// </summary>
        /// <param name="entity">Сущность статьи.</param>
        public void Delete(Article entity) =>
            _context.Articles.Remove(entity);

        /// <summary>
        /// Сохраняет все изменения в базе данных.
        /// </summary>
        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}
