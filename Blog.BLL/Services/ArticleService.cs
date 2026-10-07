using System;
using System.Collections.Generic;
using System.Text;
using Blog.BLL.DTOs;
using Blog.DAL.Entities;
using Blog.DAL.Repositories;

namespace Blog.BLL.Services
{
    /// <summary>
    /// Сервис бизнес-логики управления публикациями блога
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

        public async Task<ArticleDto> CreateAsync(CreateArticleDto dto)
        {
            // Разрешение тегов (поиск существующих или добавление новых)
            var tags = await _tagRepo.GetOrCreateTagsAsync(dto.Tags);

            var article = new Article
            {
                Title = dto.Title.Trim(),
                Content = dto.Content,
                AuthorId = dto.AuthorId,
                CreatedAt = DateTime.UtcNow,
                Tags = tags
            };

            await _articleRepo.AddAsync(article);
            await _articleRepo.SaveChangesAsync();

            // Загружаем данные для формирования полного DTO с автором
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
                a.Id,
                a.Title,
                a.Content,
                a.CreatedAt,
                a.UpdatedAt,
                a.AuthorId,
                $"{a.Author.FirstName} {a.Author.LastName}".Trim(),
                a.Tags.Select(t => t.Name).ToList(),
                a.Comments.OrderByDescending(c => c.CreatedAt).Select(c => new CommentDto(
                    c.Id,
                    c.ArticleId,
                    c.Content,
                    c.CreatedAt,
                    c.AuthorId,
                    $"{c.Author.FirstName} {c.Author.LastName}".Trim()
                )).ToList()
            );
        }

        public async Task<bool> UpdateAsync(int id, UpdateArticleDto dto)
        {
            var article = await _articleRepo.GetDetailedByIdAsync(id);
            if (article == null) return false;

            article.Title = dto.Title.Trim();
            article.Content = dto.Content;
            article.UpdatedAt = DateTime.UtcNow;

            var tags = await _tagRepo.GetOrCreateTagsAsync(dto.Tags);
            article.Tags.Clear();
            foreach (var tag in tags)
            {
                article.Tags.Add(tag);
            }

            _articleRepo.Update(article);
            await _articleRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var article = await _articleRepo.GetByIdAsync(id);
            if (article == null) return false;

            _articleRepo.Delete(article);
            await _articleRepo.SaveChangesAsync();
            return true;
        }

        private static ArticleDto MapToDto(Article a) =>
            new(
                a.Id,
                a.Title,
                a.Content,
                a.CreatedAt,
                a.UpdatedAt,
                a.AuthorId,
                $"{a.Author?.FirstName} {a.Author?.LastName}".Trim(),
                a.Tags.Select(t => t.Name).ToList(),
                a.Comments?.Count ?? 0
            );
    }
}
