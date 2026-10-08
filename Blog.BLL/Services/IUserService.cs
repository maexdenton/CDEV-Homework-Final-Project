using System;
using System.Collections.Generic;
using System.Text;
using Blog.BLL.DTOs;
using Blog.BLL.Security;

namespace Blog.BLL.Services
{
    /// <summary>
    /// Контракт сервиса управления учетными записями пользователей
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Регистрирует нового пользователя с автоматическим присвоением роли "Пользователь" (пароль хэшируется алгоритмом Identity (PBKDF2))
        /// </summary>
        Task<ServiceResult<UserDto>> RegisterAsync(RegisterUserDto dto);

        /// <summary>
        /// Возвращает всех зарегистрированных пользователей
        /// </summary>
        Task<IEnumerable<UserDto>> GetAllAsync();

        /// <summary>
        /// Возвращает профиль пользователя по его идентификатору
        /// </summary>
        Task<ServiceResult<UserDto>> GetByIdAsync(string id, string currentUserId, bool isAdmin);

        /// <summary>
        /// Обновляет личные данные пользователя
        /// </summary>
        Task<ServiceResult> UpdateAsync(string id, UpdateUserDto dto, string currentUserId, bool isAdmin);

        /// <summary>
        /// Удаляет пользователя и безопасно обрабатывает его зависимые данные
        /// </summary>
        Task<ServiceResult> DeleteAsync(string id, string currentUserId, bool isAdmin);

        /// <summary>
        /// Измененяет роли пользователя
        /// </summary>
        Task<ServiceResult> ChangeUserRoleAsync(string id, string newRole);
    }
}
