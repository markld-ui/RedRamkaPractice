using Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.ProjectId).IsRequired();
        builder.Property(m => m.UserId).IsRequired();
    }
}
