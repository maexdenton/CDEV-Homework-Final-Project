using System;
using System.Collections.Generic;
using System.Text;
using Blog.BLL.DTOs;
using Blog.BLL.Security;

namespace Blog.BLL.Services
{
    /// <summary>
    /// Контракт сервиса управления комментариями
    /// </summary>
    public interface ICommentService
    {
        Task<ServiceResult<CommentDto>> CreateAsync(CreateCommentDto dto, string currentUserId);
        Task<IEnumerable<CommentDto>> GetAllAsync();
        Task<CommentDto?> GetByIdAsync(int id);
        Task<ServiceResult> UpdateAsync(int id, UpdateCommentDto dto, string currentUserId, bool isElevated);
        Task<ServiceResult> DeleteAsync(int id, string currentUserId, bool isElevated);
    }
}
