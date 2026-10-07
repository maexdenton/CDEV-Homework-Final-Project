using System;
using System.Collections.Generic;
using System.Text;
using Blog.BLL.DTOs;

namespace Blog.BLL.Services
{
    /// <summary>
    /// Контракт сервиса управления статьями блога
    /// </summary>
    public interface IArticleService
    {
        Task<ArticleDto> CreateAsync(CreateArticleDto dto);
        Task<IEnumerable<ArticleDto>> GetAllAsync(string? search = null, string? tag = null);
        Task<IEnumerable<ArticleDto>> GetByAuthorIdAsync(string authorId);
        Task<ArticleDetailsDto?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, UpdateArticleDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
