using Blog.DAL.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Reflection.Emit;

namespace Blog.DAL
{
    // Контекст базы данных приложения.
    public class BlogDbContext : IdentityDbContext<User>
    {
        // Конструктор принимает опции (строка подключения, провайдер БД и т.д.)
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
        {
        }

        // Свойства ниже позволяют выполнять LINQ-запросы к соответствующим таблицам.
        public DbSet<Article> Articles => Set<Article>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Comment> Comments => Set<Comment>();

        // Метод вызывается EF Core при построении модели данных(схемы БД). 
        // Здесь настраиваются правила: связи, ограничения, индексы и т.д.
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Регистрация всех конфигураций IEntityTypeConfiguration из текущей сборки
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
