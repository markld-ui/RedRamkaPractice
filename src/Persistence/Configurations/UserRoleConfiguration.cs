using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

/// <summary>
/// Конфигурация сущности <see cref="UserRole"/> для Entity Framework Core.
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    /// <summary>
    /// Настраивает схему таблицы связей пользователей и ролей в базе данных.
    /// </summary>
    /// <param name="builder">Построитель конфигурации сущности.</param>
    /// <remarks>
    /// Применяемые ограничения и связи:
    /// <list type="bullet">
    ///   <item>Составной первичный ключ — (<c>UserId</c>, <c>RoleId</c>).</item>
    ///   <item><c>UserId</c> — внешний ключ, ссылающийся на <see cref="User"/>.</item>
    ///   <item><c>RoleId</c> — внешний ключ, ссылающийся на <see cref="Role"/>.</item>
    /// </list>
    /// </remarks>
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(x => new { x.UserId, x.RoleId });

        builder.HasOne(x => x.User)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.UserId);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleId);
    }
}