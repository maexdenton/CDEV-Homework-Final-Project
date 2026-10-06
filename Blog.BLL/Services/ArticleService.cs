using System;
using System.Collections.Generic;
using System.Text;
using Blog.BLL.DTOs;
using Blog.DAL.Entities;
using Blog.DAL.Repositories;

namespace Blog.BLL.Services
{
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _articleRepo;
        private readonly ITagRepository _tagRepo;
        private readonly ICommentRepository _commentRepo;

        public ArticleService(IArticleRepository articleRepo, ITagRepository tagRepo, ICommentRepository commentRepo)
        {
            _articleRepo = articleRepo;
            _tagRepo = tagRepo;
            _commentRepo = commentRepo;
        }

        public async Task<IEnumerable<ArticleDto>> GetArticlesAsync(string? search = null, string? tag = null)
        {
            var articles = await _articleRepo.GetFilteredAsync(search, tag);
            return articles.Select(a => new ArticleDto(
                a.Id,
                a.Title,
                a.Content,
                a.CreatedAt,
                a.AuthorId,
                $"{a.Author.FirstName} {a.Author.LastName}".Trim(),
                a.Tags.Select(t => t.Name).ToList(),
                a.Comments.Count
            ));
        }

        public async Task<ArticleDetailsDto?> GetArticleByIdAsync(int id)
        {
            var a = await _articleRepo.GetDetailedByIdAsync(id);
            if (a == null) return null;

            return new ArticleDetailsDto(
                a.Id,
                a.Title,
                a.Content,
                a.CreatedAt,
                a.AuthorId,
                $"{a.Author.FirstName} {a.Author.LastName}".Trim(),
                a.Tags.Select(t => t.Name).ToList(),
                a.Comments.OrderByDescending(c => c.CreatedAt).Select(c => new CommentDto(
                    c.Id,
                    c.Content,
                    c.CreatedAt,
                    $"{c.Author.FirstName} {c.Author.LastName}".Trim()
                )).ToList()
            );
        }

        public async Task<int> CreateArticleAsync(CreateArticleDto dto)
        {
            var tags = await _tagRepo.GetOrCreateTagsAsync(dto.Tags);

            var article = new Article
            {
                Title = dto.Title,
                Content = dto.Content,
                AuthorId = dto.AuthorId,
                CreatedAt = DateTime.UtcNow,
                Tags = tags
            };

            await _articleRepo.AddAsync(article);
            await _articleRepo.SaveChangesAsync();
            return article.Id;
        }

        public async Task<bool> UpdateArticleAsync(EditArticleDto dto)
        {
            var article = await _articleRepo.GetDetailedByIdAsync(dto.Id);
            if (article == null || article.AuthorId != dto.UserId)
                return false;

            article.Title = dto.Title;
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

        public async Task<bool> DeleteArticleAsync(int id, string requestingUserId)
        {
            var article = await _articleRepo.GetByIdAsync(id);
            if (article == null || article.AuthorId != requestingUserId)
                return false;

            _articleRepo.Delete(article);
            await _articleRepo.SaveChangesAsync();
            return true;
        }

        public async Task AddCommentAsync(CreateCommentDto dto)
        {
            var comment = new Comment
            {
                ArticleId = dto.ArticleId,
                Content = dto.Content,
                AuthorId = dto.AuthorId,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepo.AddAsync(comment);
            await _commentRepo.SaveChangesAsync();
        }
    }
}
