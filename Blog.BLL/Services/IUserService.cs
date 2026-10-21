using System.Collections.Generic;
using System.Threading.Tasks;
using Blog.BLL.DTOs;

namespace Blog.BLL.Services
{
    /// <summary>
    /// Контракт сервиса управления учетными записями пользователей.
    /// </summary>
    public interface IUserService
    {
        Task<ServiceResult<UserDto>> RegisterAsync(RegisterUserDto dto);

        Task<ServiceResult<IEnumerable<UserDto>>> GetAllAsync();

        Task<ServiceResult<UserDto>> GetByIdAsync(string id, string currentUserId, bool isAdmin);

        Task<ServiceResult> UpdateAsync(string id, UpdateUserDto dto, string currentUserId, bool isAdmin);

        Task<ServiceResult> DeleteAsync(string id, string currentUserId, bool isAdmin);

        Task<ServiceResult> ChangeUserRoleAsync(string id, string newRole);
    }
}
