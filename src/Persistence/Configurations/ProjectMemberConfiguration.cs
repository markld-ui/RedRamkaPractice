using Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Конфигурация сущности <see cref="ProjectMember"/> для Entity Framework Core.
/// </summary>
public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    /// <summary>
    /// Настраивает схему таблицы участников проекта в базе данных.
    /// </summary>
    /// <param name="builder">Построитель конфигурации сущности.</param>
    /// <remarks>
    /// Применяемые ограничения:
    /// <list type="bullet">
    ///   <item><c>ProjectId</c> — обязательное поле.</item>
    ///   <item><c>UserId</c> — обязательное поле.</item>
    /// </list>
    /// </remarks>
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.ProjectId).IsRequired();
        builder.Property(m => m.UserId).IsRequired();
    }
}