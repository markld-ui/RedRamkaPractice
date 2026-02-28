using Domain.Projects;
using MediatR;
using FluentValidation;
using Application.Common.Constants;
using Application.Common.Interfaces;
using Domain.Events;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Commands;

/// <summary>
/// Команда для создания нового проекта.
/// </summary>
/// <param name="Name">Название проекта.</param>
/// <param name="Description">Описание проекта.</param>
/// <param name="MemberIds">Список идентификаторов пользователей, добавляемых в проект.</param>
public record CreateProjectCommand(
    string Name,
    string Description,
    List<Guid> MemberIds) : IRequest<CreateProjectResult>;

/// <summary>
/// Результат создания проекта.
/// </summary>
/// <param name="Id">Уникальный идентификатор созданного проекта.</param>
/// <param name="Name">Название созданного проекта.</param>
/// <param name="Stage">Начальная стадия проекта.</param>
/// <param name="CreatedAt">Дата и время создания проекта в формате UTC.</param>
public record CreateProjectResult(
    Guid Id,
    string Name,
    ProjectStage Stage,
    DateTime CreatedAt);

/// <summary>
/// Валидатор команды <see cref="CreateProjectCommand"/>.
/// </summary>
public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CreateProjectCommandValidator"/>
    /// и настраивает правила валидации.
    /// </summary>
    /// <remarks>
    /// Применяемые ограничения:
    /// <list type="bullet">
    ///   <item><c>Name</c> — обязательное поле, максимум 200 символов, допустимы буквы, цифры, пробелы, дефисы и подчёркивания.</item>
    ///   <item><c>Description</c> — необязательное поле, максимум 1000 символов.</item>
    ///   <item><c>MemberIds</c> — не должны содержать пустых идентификаторов (<see cref="Guid.Empty"/>).</item>
    /// </list>
    /// </remarks>
    public CreateProjectCommandValidator()
    {
        RuleFor(n => n.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(200).WithMessage("Project name cannot exceed 200 characters.")
            .Matches(@"^[\p{L}0-9\s\-_]+$").WithMessage("Project name can only contain letters, " +
                                                         "numbers, spaces, hyphens, and underscores.");

        RuleFor(d => d.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(m => m.MemberIds)
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .WithMessage("Invalid member IDs provided.");
    }
}

/// <summary>
/// Обработчик команды <see cref="CreateProjectCommand"/>.
/// </summary>
public class CreateProjectCommandHandler :
    IRequestHandler<CreateProjectCommand, CreateProjectResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CreateProjectCommandHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    /// <param name="currentUser">Сервис для получения данных текущего пользователя.</param>
    public CreateProjectCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Обрабатывает команду создания нового проекта.
    /// </summary>
    /// <param name="request">Команда с данными создаваемого проекта.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="CreateProjectResult"/> с данными созданного проекта.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Выбрасывается, если текущий пользователь не аутентифицирован.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если часть указанных пользователей не найдена
    /// или проект с таким названием уже существует.
    /// </exception>
    /// <remarks>
    /// Создатель проекта автоматически добавляется как <c>ProjectManager</c>.
    /// Остальные участники из <c>MemberIds</c> получают роль <c>Developer</c>.
    /// Если создатель уже присутствует в <c>MemberIds</c>, дублирование исключается.
    /// </remarks>
    public async Task<CreateProjectResult> Handle(CreateProjectCommand request, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated.");

        var creatorId = _currentUser.UserId!.Value;
        var memberIds = (request.MemberIds ?? new List<Guid>())
            .Distinct()
            .ToList();

        if (!memberIds.Contains(creatorId))
            memberIds.Insert(0, creatorId);

        var users = await _context.Users
            .Where(u => memberIds.Contains(u.Id))
            .ToListAsync(ct);

        if (users.Count != memberIds.Count)
            throw new InvalidOperationException("Some users not found.");

        var roles = await _context.Roles
            .Where(r => r.Name == RoleConstants.ProjectManager
                     || r.Name == RoleConstants.Developer)
            .ToListAsync(ct);

        var projectManagerRole = roles.First(r => r.Name == RoleConstants.ProjectManager);
        var developerRole = roles.First(r => r.Name == RoleConstants.Developer);

        if (await _context.Projects.AnyAsync(p => p.Name == request.Name, ct))
            throw new InvalidOperationException("Project already exists.");

        var project = new Project(request.Name, request.Description);

        foreach (var uid in memberIds)
        {
            var role = uid == creatorId
                ? projectManagerRole
                : developerRole;

            project.AddMember(uid, role.Id);
        }

        _context.Projects.Add(project);

        await _context.SaveChangesAsync(ct);

        return new CreateProjectResult(
            project.Id,
            project.Name,
            project.Stage,
            project.CreatedAt);
    }
}