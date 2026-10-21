using System.Collections.Generic;
using System.Threading.Tasks;
using Blog.BLL.DTOs;

namespace Blog.BLL.Services
{
    /// <summary>
    /// Контракт сервиса управления тегами публикаций.
    /// </summary>
    public interface ITagService
    {
        Task<ServiceResult<TagDto>> CreateAsync(CreateTagDto dto, string currentUserId);

        Task<ServiceResult<IEnumerable<TagDto>>> GetAllAsync();

        Task<ServiceResult<TagDto>> GetByIdAsync(int id);

        Task<ServiceResult> UpdateAsync(int id, UpdateTagDto dto, string currentUserId, bool isAdmin);

        Task<ServiceResult> DeleteAsync(int id, string currentUserId, bool isAdmin);
    }
}
