using Application.Common.Interfaces;
using Domain.Common;
using Domain.Models;
using Domain.Projects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Persistence;

/// <summary>
/// Контекст базы данных приложения.
/// </summary>
/// <remarks>
/// Помимо стандартных функций EF Core, после каждого сохранения изменений
/// автоматически публикует накопленные доменные события через MediatR.
/// Конфигурации сущностей применяются из текущей сборки.
/// </remarks>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ApplicationDbContext"/>.
    /// </summary>
    /// <param name="options">Параметры конфигурации контекста базы данных.</param>
    /// <param name="mediator">Экземпляр медиатора для публикации доменных событий.</param>
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IMediator mediator)
        : base(options)
    {
        _mediator = mediator;
    }

    /// <summary>Таблица проектов.</summary>
    public DbSet<Project> Projects => Set<Project>();

    /// <summary>Таблица участников проектов.</summary>
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

    /// <summary>Таблица переходов между стадиями проектов.</summary>
    public DbSet<ProjectTransition> ProjectTransitions => Set<ProjectTransition>();

    /// <summary>Таблица спецификаций проектов.</summary>
    public DbSet<ProjectSpecification> ProjectSpecifications => Set<ProjectSpecification>();

    /// <summary>Таблица пользователей.</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>Таблица ролей.</summary>
    public DbSet<Role> Roles => Set<Role>();

    /// <summary>Таблица связей пользователей и ролей.</summary>
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    /// <summary>Таблица учётных данных пользователей.</summary>
    public DbSet<Credentials> Credentials => Set<Credentials>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Ignore<BaseEvent>();

        builder.ApplyConfigurationsFromAssembly(
            Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);
    }

    /// <summary>
    /// Сохраняет изменения в базе данных и публикует накопленные доменные события.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>
    /// Количество записей, затронутых операцией сохранения.
    /// </returns>
    /// <remarks>
    /// Публикация событий выполняется после успешного сохранения изменений.
    /// После публикации коллекция доменных событий каждой сущности очищается.
    /// </remarks>
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