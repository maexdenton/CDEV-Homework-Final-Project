using System;
using System.Collections.Generic;
using System.Text;
using Blog.BLL.DTOs;
using Blog.BLL.Security;
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

        public async Task<ServiceResult<CommentDto>> CreateAsync(CreateCommentDto dto, string currentUserId)
        {
            var article = await _articleRepo.GetByIdAsync(dto.ArticleId);
            if (article == null)
                return ServiceResult<CommentDto>.NotFound("Публикация не найдена.");

            var comment = new Comment
            {
                ArticleId = dto.ArticleId,
                AuthorId = currentUserId, // Защита: автор берется из claims
                Content = dto.Content.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepo.AddAsync(comment);
            await _commentRepo.SaveChangesAsync();

            var created = await _commentRepo.GetWithDetailsByIdAsync(comment.Id);
            return ServiceResult<CommentDto>.Ok(new CommentDto(
                created!.Id, created.ArticleId, created.Content, created.CreatedAt,
                created.AuthorId, $"{created.Author.FirstName} {created.Author.LastName}".Trim()
            ));
        }

        public async Task<IEnumerable<CommentDto>> GetAllAsync()
        {
            var comments = await _commentRepo.GetAllWithDetailsAsync();
            return comments.Select(c => new CommentDto(
                c.Id, c.ArticleId, c.Content, c.CreatedAt, c.AuthorId,
                $"{c.Author.FirstName} {c.Author.LastName}".Trim()
            ));
        }

        public async Task<CommentDto?> GetByIdAsync(int id)
        {
            var c = await _commentRepo.GetWithDetailsByIdAsync(id);
            if (c == null) return null;

            return new CommentDto(
                c.Id, c.ArticleId, c.Content, c.CreatedAt, c.AuthorId,
                $"{c.Author.FirstName} {c.Author.LastName}".Trim()
            );
        }

        public async Task<ServiceResult> UpdateAsync(int id, UpdateCommentDto dto, string currentUserId, bool isElevated)
        {
            var comment = await _commentRepo.GetByIdAsync(id);
            if (comment == null)
                return ServiceResult.NotFound("Комментарий не найден.");

            // Владелец, Модератор или Администратор
            if (comment.AuthorId != currentUserId && !isElevated)
                return ServiceResult.Forbidden("Вы можете редактировать только собственные комментарии.");

            comment.Content = dto.Content.Trim();
            _commentRepo.Update(comment);
            await _commentRepo.SaveChangesAsync();
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> DeleteAsync(int id, string currentUserId, bool isElevated)
        {
            var comment = await _commentRepo.GetByIdAsync(id);
            if (comment == null)
                return ServiceResult.NotFound("Комментарий не найден.");

            // Владелец, Модератор или Администратор
            if (comment.AuthorId != currentUserId && !isElevated)
                return ServiceResult.Forbidden("Вы не имеете права удалять данный комментарий.");

            _commentRepo.Delete(comment);
            await _commentRepo.SaveChangesAsync();
            return ServiceResult.Ok();
        }
    }
}
