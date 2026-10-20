using System;
using System.Collections.Generic;
using System.Text;
using Blog.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.DAL.Configurations
{
    /// <summary>
    /// Конфигурация Fluent API для сущности <see cref="Article"/>.
    /// Определяет ограничения полей и правила связей с другими таблицами.
    /// </summary>
    public class ArticleConfiguration : IEntityTypeConfiguration<Article>
    {
        /// <summary>
        /// Настраивает ключи, ограничения полей и связи сущности <see cref="Article"/>.
        /// </summary>
        /// <param name="builder">Построитель конфигурации сущности.</param>
        public void Configure(EntityTypeBuilder<Article> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(a => a.Content)
                .IsRequired();

            builder.HasOne(a => a.Author)
                .WithMany(u => u.Articles)
                .HasForeignKey(a => a.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(a => a.Tags)
                .WithMany(t => t.Articles)
                .UsingEntity(j => j.ToTable("ArticleTags"));
        }
    }
}
