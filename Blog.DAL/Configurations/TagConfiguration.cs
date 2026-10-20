using System;
using System.Collections.Generic;
using System.Text;
using Blog.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.DAL.Configurations
{
    /// <summary>
    /// Конфигурация Fluent API для сущности <see cref="Tag"/>.
    /// Определяет ограничения полей, уникальность имени и правила связей с другими таблицами.
    /// </summary>
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        /// <summary>
        /// Настраивает ключ, ограничения полей и связи сущности <see cref="Tag"/>.
        /// </summary>
        /// <param name="builder">Построитель конфигурации сущности.</param>
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(t => t.Name)
                .IsUnique();

            // SetNull выбран намеренно: при удалении пользователя созданные им теги
            // сохраняются в системе, а CreatorId становится NULL.
            builder.HasOne(t => t.Creator)
                .WithMany()
                .HasForeignKey(t => t.CreatorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
