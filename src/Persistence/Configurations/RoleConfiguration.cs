using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

/// <summary>
/// Конфигурация сущности <see cref="Role"/> для Entity Framework Core.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    /// <summary>
    /// Настраивает схему таблицы ролей в базе данных.
    /// </summary>
    /// <param name="builder">Построитель конфигурации сущности.</param>
    /// <remarks>
    /// Применяемые ограничения:
    /// <list type="bullet">
    ///   <item><c>Name</c> — обязательное поле, максимальная длина 100 символов, уникальный индекс.</item>
    ///   <item><c>Description</c> — необязательное поле, максимальная длина 500 символов.</item>
    /// </list>
    /// </remarks>
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}