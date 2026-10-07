using System;
using System.Collections.Generic;
using System.Text;
using Blog.BLL.DTOs;

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
        Task<(bool Succeeded, string? Error, UserDto? User)> RegisterAsync(RegisterUserDto dto);

        /// <summary>
        /// Возвращает всех зарегистрированных пользователей
        /// </summary>
        Task<IEnumerable<UserDto>> GetAllAsync();

        /// <summary>
        /// Возвращает профиль пользователя по его идентификатору
        /// </summary>
        Task<UserDto?> GetByIdAsync(string id);

        /// <summary>
        /// Обновляет личные данные пользователя
        /// </summary>
        Task<bool> UpdateAsync(string id, UpdateUserDto dto);

        /// <summary>
        /// Удаляет пользователя и безопасно обрабатывает его зависимые данные
        /// </summary>
        Task<bool> DeleteAsync(string id);
    }
}
