using Application.Common.Constants;
using Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Commands;

/// <summary>
/// Команда для добавления участника в проект с указанной ролью.
/// </summary>
/// <param name="ProjectId">Уникальный идентификатор проекта.</param>
/// <param name="UserId">Уникальный идентификатор добавляемого пользователя.</param>
/// <param name="RoleName">Название роли участника в проекте.</param>
public record AddProjectMemberCommand(
    Guid ProjectId,
    Guid UserId,
    string RoleName) : IRequest;

/// <summary>
/// Валидатор команды <see cref="AddProjectMemberCommand"/>.
/// </summary>
/// <remarks>
/// Допустимые роли участника: <c>Developer</c>, <c>Tester</c>,
/// <c>ProductManager</c>, <c>DevOps</c>.
/// Роль <c>ProjectManager</c> не может быть назначена через данную команду.
/// </remarks>
public class AddProjectMemberCommandValidator : AbstractValidator<AddProjectMemberCommand>
{
    private static readonly string[] AllowedRoles =
    [
        RoleConstants.Developer,
        RoleConstants.Tester,
        RoleConstants.ProductManager,
        RoleConstants.DevOps,
    ];

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="AddProjectMemberCommandValidator"/>
    /// и настраивает правила валидации.
    /// </summary>
    public AddProjectMemberCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty).WithMessage("Invalid user ID.");

        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Role is required.")
            .Must(r => AllowedRoles.Contains(r))
            .WithMessage($"Role must be one of: {string.Join(", ", AllowedRoles)}.");
    }
}

/// <summary>
/// Обработчик команды <see cref="AddProjectMemberCommand"/>.
/// </summary>
public class AddProjectMemberCommandHandler : IRequestHandler<AddProjectMemberCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectAuthorizationService _auth;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="AddProjectMemberCommandHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    /// <param name="currentUser">Сервис для получения данных текущего пользователя.</param>
    /// <param name="auth">Сервис проверки прав доступа к проекту.</param>
    public AddProjectMemberCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _currentUser = currentUser;
        _auth = auth;
    }

    /// <summary>
    /// Обрабатывает команду добавления участника в проект.
    /// </summary>
    /// <param name="request">Команда с данными добавляемого участника.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <exception cref="UnauthorizedAccessException">
    /// Выбрасывается, если текущий пользователь не аутентифицирован
    /// или не обладает ролью <c>ProjectManager</c> в данном проекте.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если проект, пользователь или роль не найдены,
    /// либо пользователь уже является участником проекта.
    /// </exception>
    public async Task Handle(AddProjectMemberCommand request, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated.");

        await _auth.RequireProjectRoleAsync(
            request.ProjectId, ct,
            RoleConstants.ProjectManager);

        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, ct);

        if (project is null)
            throw new InvalidOperationException($"Project {request.ProjectId} not found.");

        var userExists = await _context.Users
            .AnyAsync(u => u.Id == request.UserId, ct);

        if (!userExists)
            throw new InvalidOperationException($"User {request.UserId} not found.");

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == request.RoleName, ct);

        if (role is null)
            throw new InvalidOperationException($"Role '{request.RoleName}' not found.");

        project.AddMember(request.UserId, role.Id);
        await _context.SaveChangesAsync(ct);
    }
}