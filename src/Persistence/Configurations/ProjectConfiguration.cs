using Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
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

        builder.HasMany(p => p.Transitions)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Specifications)
            .WithOne(s => s.Project)
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
