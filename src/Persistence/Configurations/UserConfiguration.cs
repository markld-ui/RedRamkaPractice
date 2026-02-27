using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

/// <summary>
/// Конфигурация сущности <see cref="User"/> для Entity Framework Core.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// Настраивает схему таблицы пользователей в базе данных.
    /// </summary>
    /// <param name="builder">Построитель конфигурации сущности.</param>
    /// <remarks>
    /// Применяемые ограничения и связи:
    /// <list type="bullet">
    ///   <item><c>FirstName</c> — обязательное поле, максимальная длина 100 символов.</item>
    ///   <item><c>LastName</c> — обязательное поле, максимальная длина 100 символов.</item>
    ///   <item><c>Credentials</c> — связь один-к-одному с <see cref="Credentials"/>, каскадное удаление.</item>
    ///   <item><c>UserRoles</c> — связь один-ко-многим с <see cref="UserRole"/> через таблицу связей.</item>
    /// </list>
    /// </remarks>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(x => x.Credentials)
            .WithOne(c => c.User)
            .HasForeignKey<Credentials>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.UserRoles)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);
    }
}