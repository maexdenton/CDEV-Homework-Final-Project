using System;
using System.Collections.Generic;
using System.Text;
using Blog.BLL.DTOs;

namespace Blog.BLL.Services
{
    /// <summary>
    /// Контракт сервиса управления тегами публикаций
    /// </summary>
    public interface ITagService
    {
        Task<(bool Succeeded, string? Error, TagDto? Tag)> CreateAsync(CreateTagDto dto);
        Task<IEnumerable<TagDto>> GetAllAsync();
        Task<TagDto?> GetByIdAsync(int id);
        Task<(bool Succeeded, string? Error)> UpdateAsync(int id, UpdateTagDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
