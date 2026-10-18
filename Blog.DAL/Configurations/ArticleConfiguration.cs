using System;
using System.Collections.Generic;
using System.Text;
using Blog.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.DAL.Configurations
{
    // Конфигурация Fluent API для сущности "Статья".
    // Определяет ограничения полей и правила связей с другими таблицами.
    public class ArticleConfiguration : IEntityTypeConfiguration<Article>
    {
        public void Configure(EntityTypeBuilder<Article> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Title).IsRequired().HasMaxLength(250);
            builder.Property(a => a.Content).IsRequired();

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
