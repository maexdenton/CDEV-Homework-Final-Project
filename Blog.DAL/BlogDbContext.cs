using Blog.DAL.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Blog.DAL
{
    /// <summary>
    /// Контекст базы данных приложения.
    /// Предоставляет доступ к таблицам статей, тегов и комментариев,
    /// а также применяет конфигурации сущностей из текущей сборки.
    /// </summary>
    public class BlogDbContext : IdentityDbContext<User>
    {
        /// <summary>
        /// Инициализирует новый экземпляр <see cref="BlogDbContext"/>.
        /// </summary>
        /// <param name="options">Параметры конфигурации контекста (строка подключения, провайдер БД и т.д.).</param>
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Набор статей в базе данных.
        /// </summary>
        public DbSet<Article> Articles => Set<Article>();

        /// <summary>
        /// Набор тегов в базе данных.
        /// </summary>
        public DbSet<Tag> Tags => Set<Tag>();

        /// <summary>
        /// Набор комментариев в базе данных.
        /// </summary>
        public DbSet<Comment> Comments => Set<Comment>();

        /// <summary>
        /// Настраивает модель данных при построении схемы БД:
        /// связи, ограничения, индексы и другие правила.
        /// </summary>
        /// <param name="builder">Построитель модели EF Core.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
