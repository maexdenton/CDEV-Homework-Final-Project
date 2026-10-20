using System.Collections.Generic;
using System.Threading.Tasks;
using Blog.BLL.DTOs;

namespace Blog.BLL.Services
{
    /// <summary>
    /// Контракт сервиса управления статьями блога.
    /// </summary>
    public interface IArticleService
    {
        /// <summary>
        /// Создает статью. Возвращает созданный DTO, чтобы контроллер мог отдать 201 Created
        /// и ссылку на ресурс.
        /// </summary>
        Task<ArticleDto> CreateAsync(CreateArticleDto dto, string currentUserId);

        Task<IEnumerable<ArticleDto>> GetAllAsync(string? search = null, string? tag = null);

        Task<IEnumerable<ArticleDto>> GetByAuthorIdAsync(string authorId);

        Task<ArticleDetailsDto?> GetByIdAsync(int id);

        Task<ServiceResult> UpdateAsync(int id, UpdateArticleDto dto, string currentUserId, bool isElevated);

        Task<ServiceResult> DeleteAsync(int id, string currentUserId, bool isElevated);
    }
}
