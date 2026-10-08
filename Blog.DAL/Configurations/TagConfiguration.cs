using System;
using System.Collections.Generic;
using System.Text;
using Blog.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.DAL.Configurations
{
    /// <summary>
    /// Fluent API конфигурация ограничений и связей для сущности Tag
    /// </summary>
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
            builder.HasIndex(t => t.Name).IsUnique();

            // Связь с создателем: при удалении пользователя созданные им теги сохраняются (CreatorId = NULL)
            builder.HasOne(t => t.Creator)
                   .WithMany()
                   .HasForeignKey(t => t.CreatorId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
