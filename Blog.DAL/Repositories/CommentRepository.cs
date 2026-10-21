using Blog.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Blog.DAL.Repositories
{
    /// <summary>
    /// Репозиторий для работы с сущностями «Комментарий».
    /// Инкапсулирует операции чтения и записи комментариев в БД.
    /// </summary>
    public class CommentRepository : ICommentRepository
    {
        private readonly BlogDbContext _context;

        public CommentRepository(BlogDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Возвращает все комментарии без отслеживания изменений.
        /// </summary>
        /// <returns>Коллекция комментариев.</returns>
        public async Task<IEnumerable<Comment>> GetAllAsync() =>
            await _context.Comments
                .AsNoTracking()
                .ToListAsync();

        /// <summary>
        /// Возвращает все комментарии с загруженными авторами и статьями,
        /// отсортированные по дате создания (сначала новые).
        /// </summary>
        /// <returns>Коллекция комментариев с деталями.</returns>
        public async Task<IEnumerable<Comment>> GetAllWithDetailsAsync() =>
            await _context.Comments
                .Include(c => c.Author)
                .Include(c => c.Article)
                .OrderByDescending(c => c.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

        /// <summary>
        /// Возвращает комментарий по идентификатору без загрузки связанных данных.
        /// </summary>
        /// <param name="id">Идентификатор комментария.</param>
        /// <returns>Комментарий или null, если не найден.</returns>
        public async Task<Comment?> GetByIdAsync(int id) =>
            await _context.Comments.FindAsync(id);

        /// <summary>
        /// Возвращает комментарий с загруженными автором и статьей.
        /// </summary>
        /// <param name="id">Идентификатор комментария.</param>
        /// <returns>Комментарий с деталями или null, если не найден.</returns>
        public async Task<Comment?> GetWithDetailsByIdAsync(int id) =>
            await _context.Comments
                .Include(c => c.Author)
                .Include(c => c.Article)
                .FirstOrDefaultAsync(c => c.Id == id);

        /// <summary>
        /// Добавляет новый комментарий в контекст (без сохранения).
        /// </summary>
        /// <param name="entity">Сущность комментария.</param>
        public async Task AddAsync(Comment entity) =>
            await _context.Comments.AddAsync(entity);

        /// <summary>
        /// Помечает комментарий как измененный.
        /// </summary>
        /// <param name="entity">Сущность комментария.</param>
        public void Update(Comment entity) =>
            _context.Comments.Update(entity);

        /// <summary>
        /// Помечает комментарий как удаленный.
        /// </summary>
        /// <param name="entity">Сущность комментария.</param>
        public void Delete(Comment entity) =>
            _context.Comments.Remove(entity);

        /// <summary>
        /// Сохраняет все изменения в базе данных.
        /// </summary>
        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}
