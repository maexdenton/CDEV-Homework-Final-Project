using System;
using System.Collections.Generic;
using System.Text;
using Blog.BLL.DTOs;
using Blog.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog.BLL.Services
{
    /// <summary>
    /// Сервис управления пользователями, регистрацией и ролевой политикой
    /// </summary>
    public class UserService : IUserService
    {
        public const string DefaultRole = "Пользователь";

        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <inheritdoc />
        public async Task<(bool Succeeded, string? Error, UserDto? User)> RegisterAsync(RegisterUserDto dto)
        {
            // Проверка на существование учетной записи с данным Email
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return (false, "Пользователь с указанным адресом электронной почты уже зарегистрирован.", null);
            }

            // Гарантия существования базовой роли "Пользователь" в системе (авто-создание при отсутствии)
            if (!await _roleManager.RoleExistsAsync(DefaultRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(DefaultRole));
            }

            // Формирование сущности пользователя
            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                RegisteredAt = DateTime.UtcNow
            };

            // Безопасное создание пользователя.
            // UserManager автоматически хэширует пароль с добавлением соли через IPasswordHasher
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return (false, errors, null);
            }

            // Автоматическое назначение базовой роли "Пользователь"
            await _userManager.AddToRoleAsync(user, DefaultRole);

            var roles = await _userManager.GetRolesAsync(user);
            var createdDto = new UserDto(
                user.Id,
                user.Email!,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.RegisteredAt,
                roles
            );

            return (true, null, createdDto);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public async Task<UserDto?> GetByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, user.PhoneNumber, user.RegisteredAt, roles);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(string id, UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.PhoneNumber = dto.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            // Удаление пользователя через UserManager.
            // В соответствии с конфигурацией BlogDbContext, статьи удалятся каскадно
            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }
    }
}
