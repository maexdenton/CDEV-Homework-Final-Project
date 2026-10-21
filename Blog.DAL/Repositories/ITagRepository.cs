using System.Collections.Generic;
using System.Threading.Tasks;
using Blog.DAL.Entities;

namespace Blog.DAL.Repositories
{
    /// <summary>
    /// Контракт репозитория для работы с сущностями «Тег».
    /// Отвечает за управление тегами и их связями со статьями (связь «многие-ко-многим»).
    /// </summary>
    public interface ITagRepository : IRepository<Tag>
    {
        /// <summary>
        /// Возвращает тег по его имени.
        /// </summary>
        /// <param name="name">Имя тега.</param>
        /// <returns>Тег или null, если тег с таким именем не найден.</returns>
        Task<Tag?> GetByNameAsync(string name);

        /// <summary>
        /// Разрешает список имен тегов: возвращает существующие теги и создает отсутствующие,
        /// фиксируя идентификатор создателя.
        /// </summary>
        /// <param name="tagNames">Коллекция имен тегов.</param>
        /// <param name="creatorId">Идентификатор пользователя, создающего отсутствующие теги (необязательно).</param>
        /// <returns>Коллекция сущностей тегов, соответствующих переданным именам.</returns>
        Task<IEnumerable<Tag>> GetOrCreateTagsAsync(IEnumerable<string> tagNames, string? creatorId = null);
    }
}
