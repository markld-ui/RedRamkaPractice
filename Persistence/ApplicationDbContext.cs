using Domain.Models;
using Domain.Projects;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Credentials> Credentials => Set<Credentials>();

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectTransition> ProjectTransitions => Set<ProjectTransition>();
    public DbSet<ProjectSpecification> ProjectSpecifications => Set<ProjectSpecification>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(
            Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);
    }
}
