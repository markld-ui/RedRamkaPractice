using Domain.Models;
using Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

/// <summary>
/// Интерфейс контекста базы данных приложения.
/// </summary>
/// <remarks>
/// Абстрагирует доступ к данным для слоя приложения,
/// позволяя подменять реализацию в тестах без зависимости от EF Core.
/// </remarks>
public interface IApplicationDbContext
{
    /// <summary>Таблица проектов.</summary>
    DbSet<Project> Projects { get; }

    /// <summary>Таблица участников проектов.</summary>
    DbSet<ProjectMember> ProjectMembers { get; }

    /// <summary>Таблица переходов между стадиями проектов.</summary>
    DbSet<ProjectTransition> ProjectTransitions { get; }

    /// <summary>Таблица спецификаций проектов.</summary>
    DbSet<ProjectSpecification> ProjectSpecifications { get; }

    /// <summary>Таблица пользователей.</summary>
    DbSet<User> Users { get; }

    /// <summary>Таблица ролей.</summary>
    DbSet<Role> Roles { get; }

    /// <summary>Таблица связей пользователей и ролей.</summary>
    DbSet<UserRole> UserRoles { get; }

    /// <summary>Таблица учётных данных пользователей.</summary>
    DbSet<Credentials> Credentials { get; }

    /// <summary>
    /// Асинхронно сохраняет все изменения, внесённые в контекст, в базу данных.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Количество записей, затронутых операцией сохранения.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}