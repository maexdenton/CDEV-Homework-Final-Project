using System;
using System.Collections.Generic;
using System.Text;
using Blog.BLL.DTOs;
using Blog.DAL.Entities;
using Blog.DAL.Repositories;

namespace Blog.BLL.Services
{
    /// <summary>
    /// Сервис бизнес-логики управления комментариями
    /// </summary>
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepo;
        private readonly IArticleRepository _articleRepo;

        public CommentService(ICommentRepository commentRepo, IArticleRepository articleRepo)
        {
            _commentRepo = commentRepo;
            _articleRepo = articleRepo;
        }

        public async Task<(bool Succeeded, string? Error, CommentDto? Comment)> CreateAsync(CreateCommentDto dto)
        {
            // Проверка существования целевой статьи
            var article = await _articleRepo.GetByIdAsync(dto.ArticleId);
            if (article == null)
            {
                return (false, "Статья с указанным идентификатором не существует.", null);
            }

            var comment = new Comment
            {
                ArticleId = dto.ArticleId,
                AuthorId = dto.AuthorId,
                Content = dto.Content.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepo.AddAsync(comment);
            await _commentRepo.SaveChangesAsync();

            var created = await _commentRepo.GetWithDetailsByIdAsync(comment.Id);
            var commentDto = new CommentDto(
                created!.Id,
                created.ArticleId,
                created.Content,
                created.CreatedAt,
                created.AuthorId,
                $"{created.Author.FirstName} {created.Author.LastName}".Trim()
            );

            return (true, null, commentDto);
        }

        public async Task<IEnumerable<CommentDto>> GetAllAsync()
        {
            var comments = await _commentRepo.GetAllWithDetailsAsync();
            return comments.Select(c => new CommentDto(
                c.Id,
                c.ArticleId,
                c.Content,
                c.CreatedAt,
                c.AuthorId,
                $"{c.Author.FirstName} {c.Author.LastName}".Trim()
            ));
        }

        public async Task<CommentDto?> GetByIdAsync(int id)
        {
            var c = await _commentRepo.GetWithDetailsByIdAsync(id);
            if (c == null) return null;

            return new CommentDto(
                c.Id,
                c.ArticleId,
                c.Content,
                c.CreatedAt,
                c.AuthorId,
                $"{c.Author.FirstName} {c.Author.LastName}".Trim()
            );
        }

        public async Task<bool> UpdateAsync(int id, UpdateCommentDto dto)
        {
            var comment = await _commentRepo.GetByIdAsync(id);
            if (comment == null) return false;

            comment.Content = dto.Content.Trim();
            _commentRepo.Update(comment);
            await _commentRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var comment = await _commentRepo.GetByIdAsync(id);
            if (comment == null) return false;

            _commentRepo.Delete(comment);
            await _commentRepo.SaveChangesAsync();
            return true;
        }
    }
}
