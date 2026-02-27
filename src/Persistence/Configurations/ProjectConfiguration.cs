using Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Конфигурация сущности <see cref="Project"/> для Entity Framework Core.
/// </summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    /// <summary>
    /// Настраивает схему таблицы проектов в базе данных.
    /// </summary>
    /// <param name="builder">Построитель конфигурации сущности.</param>
    /// <remarks>
    /// Применяемые ограничения и связи:
    /// <list type="bullet">
    ///   <item><c>Name</c> — обязательное поле, максимальная длина 200 символов.</item>
    ///   <item><c>Members</c> — связь один-ко-многим с <see cref="ProjectMember"/>, каскадное удаление, доступ через приватное поле <c>_members</c>.</item>
    ///   <item><c>Transitions</c> — связь один-ко-многим с <see cref="ProjectTransition"/>, каскадное удаление, доступ через приватное поле <c>_transitions</c>.</item>
    ///   <item><c>Specifications</c> — связь один-ко-многим с <see cref="ProjectSpecification"/>, каскадное удаление, доступ через приватное поле <c>_specifications</c>.</item>
    /// </list>
    /// </remarks>
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasMany(p => p.Members)
            .WithOne(m => m.Project)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Members)
            .HasField("_members")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.Transitions)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Transitions)
            .HasField("_transitions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.Specifications)
            .WithOne(s => s.Project)
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Specifications)
            .HasField("_specifications")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}