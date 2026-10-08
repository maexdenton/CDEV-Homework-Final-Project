using System;
using System.Collections.Generic;
using System.Text;
using Blog.BLL.DTOs;
using Blog.BLL.Security;

namespace Blog.BLL.Services
{
    /// <summary>
    /// Контракт сервиса управления тегами публикаций
    /// </summary>
    public interface ITagService
    {
        Task<ServiceResult<TagDto>> CreateAsync(CreateTagDto dto, string currentUserId);
        Task<IEnumerable<TagDto>> GetAllAsync();
        Task<TagDto?> GetByIdAsync(int id);
        Task<ServiceResult> UpdateAsync(int id, UpdateTagDto dto, string currentUserId, bool isAdmin);
        Task<ServiceResult> DeleteAsync(int id, string currentUserId, bool isAdmin);
    }
}
