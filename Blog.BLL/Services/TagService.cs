using System;
using System.Collections.Generic;
using System.Text;
using Blog.BLL.DTOs;
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
        private readonly IArticleRepository _articleRepo;

        public TagService(ITagRepository tagRepo, IArticleRepository articleRepo)
        {
            _tagRepo = tagRepo;
            _articleRepo = articleRepo;
        }

        public async Task<(bool Succeeded, string? Error, TagDto? Tag)> CreateAsync(CreateTagDto dto)
        {
            var cleanName = dto.Name.Trim().ToLower();

            // Проверка уникальности тега
            var existing = await _tagRepo.GetByNameAsync(cleanName);
            if (existing != null)
            {
                return (false, "Тег с таким наименованием уже существует.", null);
            }

            var tag = new Tag { Name = cleanName };
            await _tagRepo.AddAsync(tag);
            await _tagRepo.SaveChangesAsync();

            return (true, null, new TagDto(tag.Id, tag.Name, 0));
        }

        public async Task<IEnumerable<TagDto>> GetAllAsync()
        {
            var tags = await _tagRepo.GetAllAsync();
            return tags.Select(t => new TagDto(t.Id, t.Name, t.Articles.Count));
        }

        public async Task<TagDto?> GetByIdAsync(int id)
        {
            var tag = await _tagRepo.GetByIdAsync(id);
            if (tag == null) return null;

            return new TagDto(tag.Id, tag.Name, tag.Articles.Count);
        }

        public async Task<(bool Succeeded, string? Error)> UpdateAsync(int id, UpdateTagDto dto)
        {
            var tag = await _tagRepo.GetByIdAsync(id);
            if (tag == null) return (false, "Тег не найден.");

            var cleanName = dto.Name.Trim().ToLower();
            var existing = await _tagRepo.GetByNameAsync(cleanName);
            if (existing != null && existing.Id != id)
            {
                return (false, "Другой тег с таким именем уже существует.");
            }

            tag.Name = cleanName;
            _tagRepo.Update(tag);
            await _tagRepo.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var tag = await _tagRepo.GetByIdAsync(id);
            if (tag == null) return false;

            _tagRepo.Delete(tag);
            await _tagRepo.SaveChangesAsync();
            return true;
        }
    }
}
