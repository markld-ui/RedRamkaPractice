using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

/// <summary>
/// Конфигурация сущности <see cref="Credentials"/> для Entity Framework Core.
/// </summary>
public class CredentialsConfiguration : IEntityTypeConfiguration<Credentials>
{
    /// <summary>
    /// Настраивает схему таблицы учётных данных в базе данных.
    /// </summary>
    /// <param name="builder">Построитель конфигурации сущности.</param>
    /// <remarks>
    /// Применяемые ограничения:
    /// <list type="bullet">
    ///   <item><c>Email</c> — обязательное поле, максимальная длина 200 символов, уникальный индекс.</item>
    ///   <item><c>PasswordHash</c> — обязательное поле.</item>
    ///   <item><c>RefreshToken</c> — необязательное поле.</item>
    /// </list>
    /// </remarks>
    public void Configure(EntityTypeBuilder<Credentials> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .IsRequired();

        builder.Property(x => x.RefreshToken)
            .IsRequired(false);
    }
}