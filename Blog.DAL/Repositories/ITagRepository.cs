using Blog.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.DAL.Repositories
{
    // Интерфейс репозитория для управления сущностями "Тег".
    // Отвечает за работу с тегами и их связями со статьями (связь "многие-ко-многим").
    public interface ITagRepository : IRepository<Tag>
    {
        Task<Tag?> GetByNameAsync(string name);

        // Разрешает список тегов: находит существующие и создает отсутствующие с фиксацией автора
        Task<List<Tag>> GetOrCreateTagsAsync(IEnumerable<string> tagNames, string? creatorId = null);
    }
}
