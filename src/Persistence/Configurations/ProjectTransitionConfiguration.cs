using Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Конфигурация сущности <see cref="ProjectTransition"/> для Entity Framework Core.
/// </summary>
public class ProjectTransitionConfiguration : IEntityTypeConfiguration<ProjectTransition>
{
    /// <summary>
    /// Настраивает схему таблицы переходов между стадиями проекта в базе данных.
    /// </summary>
    /// <param name="builder">Построитель конфигурации сущности.</param>
    /// <remarks>
    /// Применяемые ограничения:
    /// <list type="bullet">
    ///   <item><c>FromStage</c> — обязательное поле.</item>
    ///   <item><c>ToStage</c> — обязательное поле.</item>
    /// </list>
    /// </remarks>
    public void Configure(EntityTypeBuilder<ProjectTransition> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.FromStage).IsRequired();
        builder.Property(t => t.ToStage).IsRequired();
    }
}