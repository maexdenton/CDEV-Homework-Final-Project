using System;
using System.Collections.Generic;
using System.Text;
using Blog.BLL.DTOs;

namespace Blog.BLL.Services
{
    /// <summary>
    /// Контракт сервиса управления комментариями
    /// </summary>
    public interface ICommentService
    {
        Task<(bool Succeeded, string? Error, CommentDto? Comment)> CreateAsync(CreateCommentDto dto);
        Task<IEnumerable<CommentDto>> GetAllAsync();
        Task<CommentDto?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, UpdateCommentDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
