using Blog.BLL.DTOs;
using Blog.BLL.Security;
using Blog.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.BLL.Services
{
    /// <summary>
    /// Сервис управления пользователями с проверкой полномочий и предотвращением эскалации привилегий
    /// </summary>
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Регистрация нового пользователя с автоматическим присвоением роли "Пользователь"
        /// </summary>
        public async Task<ServiceResult<UserDto>> RegisterAsync(RegisterUserDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return ServiceResult<UserDto>.Conflict("Пользователь с таким email уже зарегистрирован.");

            // Гарантия наличия роли "Пользователь"
            if (!await _roleManager.RoleExistsAsync(AppRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(AppRoles.User));

            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                RegisteredAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return ServiceResult<UserDto>.BadRequest(string.Join("; ", result.Errors.Select(e => e.Description)));

            // Автоматически назначаем базовую роль "Пользователь"
            await _userManager.AddToRoleAsync(user, AppRoles.User);

            var roles = await _userManager.GetRolesAsync(user);
            return ServiceResult<UserDto>.Ok(new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.PhoneNumber, user.RegisteredAt, roles));
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userManager.Users.AsNoTracking().ToListAsync();
            var result = new List<UserDto>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new UserDto(u.Id, u.Email!, u.FirstName, u.LastName, u.PhoneNumber, u.RegisteredAt, roles));
            }
            return result;
        }

        public async Task<ServiceResult<UserDto>> GetByIdAsync(string id, string currentUserId, bool isAdmin)
        {
            // Пользователь может запрашивать только свой профиль, если он не Администратор
            if (id != currentUserId && !isAdmin)
                return ServiceResult<UserDto>.Forbidden("Доступ к чужому профилю ограничен.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return ServiceResult<UserDto>.NotFound("Пользователь не найден.");

            var roles = await _userManager.GetRolesAsync(user);
            return ServiceResult<UserDto>.Ok(new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, user.PhoneNumber, user.RegisteredAt, roles));
        }

        public async Task<ServiceResult> UpdateAsync(string id, UpdateUserDto dto, string currentUserId, bool isAdmin)
        {
            // Редактировать профиль может только владелец или Администратор
            if (id != currentUserId && !isAdmin)
                return ServiceResult.Forbidden("Вы можете редактировать только собственный профиль.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return ServiceResult.NotFound("Пользователь не найден.");

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.PhoneNumber = dto.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded ? ServiceResult.Ok() : ServiceResult.BadRequest("Не удалось обновить профиль.");
        }

        public async Task<ServiceResult> DeleteAsync(string id, string currentUserId, bool isAdmin)
        {
            // Удалять пользователей может исключительно Администратор
            if (!isAdmin)
                return ServiceResult.Forbidden("Удаление учетных записей разрешено только Администратору.");

            // Защита от случайного удаления собственной учетной записи администратором
            if (id == currentUserId)
                return ServiceResult.BadRequest("Администратор не может удалить сам себя.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return ServiceResult.NotFound("Пользователь не найден.");

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded ? ServiceResult.Ok() : ServiceResult.BadRequest("Ошибка при удалении пользователя.");
        }

        /// <summary>
        /// Смена роли пользователя. Доступна только с правом Roles.Manage
        /// </summary>
        public async Task<ServiceResult> ChangeUserRoleAsync(string id, string newRole)
        {
            if (!await _roleManager.RoleExistsAsync(newRole))
                return ServiceResult.BadRequest($"Роль '{newRole}' не существует в системе.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return ServiceResult.NotFound("Пользователь не найден.");

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            return ServiceResult.Ok();
        }
    }
}
