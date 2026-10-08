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
    /// Сервис статей с защитой от подмены автора и проверкой владения
    /// </summary>
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _articleRepo;
        private readonly ITagRepository _tagRepo;

        public ArticleService(IArticleRepository articleRepo, ITagRepository tagRepo)
        {
            _articleRepo = articleRepo;
            _tagRepo = tagRepo;
        }

        public async Task<ArticleDto> CreateAsync(CreateArticleDto dto, string currentUserId)
        {
            // Идентификатор автора жестко привязывается к текущему пользователю из claims
            var tags = await _tagRepo.GetOrCreateTagsAsync(dto.Tags, currentUserId);

            var article = new Article
            {
                Title = dto.Title.Trim(),
                Content = dto.Content,
                AuthorId = currentUserId, // Защита от Over-Posting / подмены автора
                CreatedAt = DateTime.UtcNow,
                Tags = tags
            };

            await _articleRepo.AddAsync(article);
            await _articleRepo.SaveChangesAsync();

            var created = await _articleRepo.GetDetailedByIdAsync(article.Id);
            return MapToDto(created!);
        }

        public async Task<IEnumerable<ArticleDto>> GetAllAsync(string? search = null, string? tag = null)
        {
            var articles = await _articleRepo.GetFilteredAsync(search, tag);
            return articles.Select(MapToDto);
        }

        public async Task<IEnumerable<ArticleDto>> GetByAuthorIdAsync(string authorId)
        {
            var articles = await _articleRepo.GetByAuthorIdAsync(authorId);
            return articles.Select(MapToDto);
        }

        public async Task<ArticleDetailsDto?> GetByIdAsync(int id)
        {
            var a = await _articleRepo.GetDetailedByIdAsync(id);
            if (a == null) return null;

            return new ArticleDetailsDto(
                a.Id, a.Title, a.Content, a.CreatedAt, a.UpdatedAt, a.AuthorId,
                $"{a.Author.FirstName} {a.Author.LastName}".Trim(),
                a.Tags.Select(t => t.Name).ToList(),
                a.Comments.OrderByDescending(c => c.CreatedAt).Select(c => new CommentDto(
                    c.Id, c.ArticleId, c.Content, c.CreatedAt, c.AuthorId,
                    $"{c.Author.FirstName} {c.Author.LastName}".Trim()
                )).ToList()
            );
        }

        public async Task<ServiceResult> UpdateAsync(int id, UpdateArticleDto dto, string currentUserId, bool isElevated)
        {
            var article = await _articleRepo.GetDetailedByIdAsync(id);
            if (article == null)
                return ServiceResult.NotFound("Статья не найдена.");

            // Владелец, Модератор или Администратор имеют право на редактирование
            if (article.AuthorId != currentUserId && !isElevated)
                return ServiceResult.Forbidden("Вы не являетесь автором этой статьи и не имеете прав модератора.");

            article.Title = dto.Title.Trim();
            article.Content = dto.Content;
            article.UpdatedAt = DateTime.UtcNow;

            var tags = await _tagRepo.GetOrCreateTagsAsync(dto.Tags, currentUserId);
            article.Tags.Clear();
            foreach (var tag in tags)
            {
                article.Tags.Add(tag);
            }

            _articleRepo.Update(article);
            await _articleRepo.SaveChangesAsync();
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> DeleteAsync(int id, string currentUserId, bool isElevated)
        {
            var article = await _articleRepo.GetByIdAsync(id);
            if (article == null)
                return ServiceResult.NotFound("Статья не найдена.");

            // Удалять может автор, Модератор или Администратор
            if (article.AuthorId != currentUserId && !isElevated)
                return ServiceResult.Forbidden("Вы не можете удалить чужую публикацию.");

            _articleRepo.Delete(article);
            await _articleRepo.SaveChangesAsync();
            return ServiceResult.Ok();
        }

        private static ArticleDto MapToDto(Article a) =>
            new(
                a.Id, a.Title, a.Content, a.CreatedAt, a.UpdatedAt, a.AuthorId,
                $"{a.Author?.FirstName} {a.Author?.LastName}".Trim(),
                a.Tags.Select(t => t.Name).ToList(),
                a.Comments?.Count ?? 0
            );
    }
}
