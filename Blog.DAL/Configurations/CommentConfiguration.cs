using System;
using System.Collections.Generic;
using System.Text;
using Blog.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.DAL.Configurations
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Content).IsRequired().HasMaxLength(1500);

            // Каскадное удаление: удалили статью -> удалились комментарии
            builder.HasOne(c => c.Article)
                   .WithMany(a => a.Comments)
                   .HasForeignKey(c => c.ArticleId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Защита от Multiple Cascade Paths (SQL Server Error 1785)
            // При удалении пользователя его комментарии не удаляются каскадно, чтобы избежать конфликта с каскадом от статьи
            builder.HasOne(c => c.Author)
                   .WithMany(u => u.Comments)
                   .HasForeignKey(c => c.AuthorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
