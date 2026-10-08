using System;
using System.Collections.Generic;
using System.Text;
using Blog.BLL.DTOs;
using Blog.BLL.Security;
using Blog.DAL.Entities;
using Blog.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Blog.BLL.Services
{
    /// <summary>
    /// Сервис бизнес-логики управления тегами
    /// </summary>
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepo;

        public TagService(ITagRepository tagRepo)
        {
            _tagRepo = tagRepo;
        }

        public async Task<ServiceResult<TagDto>> CreateAsync(CreateTagDto dto, string currentUserId)
        {
            var cleanName = dto.Name.Trim().ToLower();
            var existing = await _tagRepo.GetByNameAsync(cleanName);
            if (existing != null)
                return ServiceResult<TagDto>.Conflict("Тег с таким именем уже существует.");

            var tag = new Tag { Name = cleanName, CreatorId = currentUserId };
            await _tagRepo.AddAsync(tag);
            await _tagRepo.SaveChangesAsync();

            return ServiceResult<TagDto>.Ok(new TagDto(tag.Id, tag.Name, tag.CreatorId, 0));
        }

        public async Task<IEnumerable<TagDto>> GetAllAsync()
        {
            var tags = await _tagRepo.GetAllAsync();
            return tags.Select(t => new TagDto(t.Id, t.Name, t.CreatorId, t.Articles.Count));
        }

        public async Task<TagDto?> GetByIdAsync(int id)
        {
            var tag = await _tagRepo.GetByIdAsync(id);
            if (tag == null) return null;

            return new TagDto(tag.Id, tag.Name, tag.CreatorId, tag.Articles.Count);
        }

        public async Task<ServiceResult> UpdateAsync(int id, UpdateTagDto dto, string currentUserId, bool isAdmin)
        {
            var tag = await _tagRepo.GetByIdAsync(id);
            if (tag == null)
                return ServiceResult.NotFound("Тег не найден.");

            // Модератор НЕ имеет права редактировать теги. Только создатель тега или Администратор
            if (tag.CreatorId != currentUserId && !isAdmin)
                return ServiceResult.Forbidden("Вы можете редактировать только теги собственного авторства.");

            var cleanName = dto.Name.Trim().ToLower();
            var existing = await _tagRepo.GetByNameAsync(cleanName);
            if (existing != null && existing.Id != id)
                return ServiceResult.Conflict("Другой тег с таким именем уже существует.");

            tag.Name = cleanName;
            _tagRepo.Update(tag);
            await _tagRepo.SaveChangesAsync();
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> DeleteAsync(int id, string currentUserId, bool isAdmin)
        {
            var tag = await _tagRepo.GetByIdAsync(id);
            if (tag == null)
                return ServiceResult.NotFound("Тег не найден.");

            // Модератор НЕ имеет права удалять теги. Только создатель или Администратор
            if (tag.CreatorId != currentUserId && !isAdmin)
                return ServiceResult.Forbidden("Вы можете удалять только теги собственного авторства.");

            _tagRepo.Delete(tag);
            await _tagRepo.SaveChangesAsync();
            return ServiceResult.Ok();
        }
    }
}
