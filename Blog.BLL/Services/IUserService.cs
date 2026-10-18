using System;
using System.Collections.Generic;
using System.Text;
using Blog.BLL.DTOs;
using Blog.BLL.Security;

namespace Blog.BLL.Services
{
    // Контракт сервиса управления учетными записями пользователей
    public interface IUserService
    {
        // Регистрирует нового пользователя с автоматическим присвоением роли "Пользователь" (пароль хэшируется алгоритмом Identity (PBKDF2))
        Task<ServiceResult<UserDto>> RegisterAsync(RegisterUserDto dto);

        // Возвращает всех зарегистрированных пользователей
        Task<IEnumerable<UserDto>> GetAllAsync();

        // Возвращает профиль пользователя по его идентификатору
        Task<ServiceResult<UserDto>> GetByIdAsync(string id, string currentUserId, bool isAdmin);

        // Обновляет личные данные пользователя
        Task<ServiceResult> UpdateAsync(string id, UpdateUserDto dto, string currentUserId, bool isAdmin);

        // Удаляет пользователя и безопасно обрабатывает его зависимые данные
        Task<ServiceResult> DeleteAsync(string id, string currentUserId, bool isAdmin);

        // Измененяет роли пользователя
        Task<ServiceResult> ChangeUserRoleAsync(string id, string newRole);
    }
}
