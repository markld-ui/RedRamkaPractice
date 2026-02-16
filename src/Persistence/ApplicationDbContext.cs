using Application.Common.Interfaces;
using Domain.Common;
using Domain.Models;
using Domain.Projects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly IMediator _mediator;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IMediator mediator)
        : base(options)
    {
        _mediator = mediator;
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<ProjectTransition> ProjectTransitions => Set<ProjectTransition>();
    public DbSet<ProjectSpecification> ProjectSpecifications => Set<ProjectSpecification>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<Credentials> Credentials => Set<Credentials>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Ignore<BaseEvent>();

        builder.ApplyConfigurationsFromAssembly(
            Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var entitiesWithEvents = ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var entity in entitiesWithEvents)
        {
            foreach (var domainEvent in entity.DomainEvents)
                await _mediator.Publish(domainEvent, cancellationToken);

            entity.ClearDomainEvents();
        }

        return result;
    }
}
