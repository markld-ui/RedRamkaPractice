using Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProjectTransitionConfiguration : IEntityTypeConfiguration<ProjectTransition>
{
    public void Configure(EntityTypeBuilder<ProjectTransition> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.FromStage).IsRequired();
        builder.Property(t => t.ToStage).IsRequired();
    }
}
