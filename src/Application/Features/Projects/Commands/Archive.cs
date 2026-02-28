using Application.Common.Constants;
using Application.Common.Interfaces;
using Application.Features.Projects.Commands.Shared;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Commands;

/// <summary>
/// Команда для архивирования проекта с указанием причины.
/// </summary>
/// <param name="ProjectId">Уникальный идентификатор проекта.</param>
/// <param name="Reason">Причина архивирования проекта.</param>
public record ArchiveCommand(Guid ProjectId, string Reason) : IRequest<TransitionResult>;

/// <summary>
/// Валидатор команды <see cref="ArchiveCommand"/>.
/// </summary>
public class ArchiveCommandValidator : AbstractValidator<ArchiveCommand>
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ArchiveCommandValidator"/>
    /// и настраивает правила валидации.
    /// </summary>
    /// <remarks>
    /// Причина архивирования обязательна и не может превышать 1000 символов.
    /// </remarks>
    public ArchiveCommandValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required when archiving a project.")
            .MaximumLength(1000).WithMessage("Reason cannot exceed 1000 characters.");
    }
}

/// <summary>
/// Обработчик команды <see cref="ArchiveCommand"/>.
/// </summary>
public class ArchiveCommandHandler : IRequestHandler<ArchiveCommand, TransitionResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectAuthorizationService _auth;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ArchiveCommandHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    /// <param name="currentUser">Сервис для получения данных текущего пользователя.</param>
    /// <param name="auth">Сервис проверки прав доступа к проекту.</param>
    public ArchiveCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _currentUser = currentUser;
        _auth = auth;
    }

    /// <summary>
    /// Обрабатывает команду архивирования проекта.
    /// </summary>
    /// <param name="request">Команда с идентификатором проекта и причиной архивирования.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="TransitionResult"/> с новой стадией <c>Archived</c> при успехе,
    /// либо с описанием ошибки если переход невозможен из текущей стадии.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Выбрасывается, если текущий пользователь не аутентифицирован
    /// или не обладает ролью <c>ProjectManager</c> в данном проекте.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если проект не найден.
    /// </exception>
    public async Task<TransitionResult> Handle(ArchiveCommand request, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated.");

        await _auth.RequireProjectRoleAsync(
            request.ProjectId, ct,
            RoleConstants.ProjectManager);

        var project = await _context.Projects
            .Include(p => p.Transitions)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, ct);

        if (project is null)
            throw new InvalidOperationException($"Project {request.ProjectId} not found.");

        var result = project.Archive(request.Reason);

        if (!result.IsSuccess)
            return new TransitionResult(false, result.Error, null);

        var newTransition = project.Transitions
            .OrderByDescending(t => t.ChangedAt)
            .First();
        _context.ProjectTransitions.Add(newTransition);

        await _context.SaveChangesAsync(ct);
        return new TransitionResult(true, null, result.NewStage!.Value.ToString());
    }
}