using System;
using System.Collections.Generic;
using System.Text;
using Blog.BLL.DTOs;

namespace Blog.BLL.Services
{
    public interface IArticleService
    {
        Task<IEnumerable<ArticleDto>> GetArticlesAsync(string? search = null, string? tag = null);
        Task<ArticleDetailsDto?> GetArticleByIdAsync(int id);
        Task<int> CreateArticleAsync(CreateArticleDto dto);
        Task<bool> UpdateArticleAsync(EditArticleDto dto);
        Task<bool> DeleteArticleAsync(int id, string requestingUserId);
        Task AddCommentAsync(CreateCommentDto dto);
    }
}
