using Domain.Models;
using Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Project> Projects { get; }
    DbSet<ProjectMember> ProjectMembers { get; }
    DbSet<ProjectTransition> ProjectTransitions { get; }
    DbSet<ProjectSpecification> ProjectSpecifications { get; }

    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Credentials> Credentials { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
