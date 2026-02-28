using Application.Common.Constants;
using Application.Common.Interfaces;
using Application.Features.Projects.Commands.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Commands;

/// <summary>
/// Команда для перевода проекта в стадию релиза.
/// </summary>
/// <param name="ProjectId">Уникальный идентификатор проекта.</param>
public record ReleaseCommand(Guid ProjectId) : IRequest<TransitionResult>;

/// <summary>
/// Обработчик команды <see cref="ReleaseCommand"/>.
/// </summary>
public class ReleaseCommandHandler : IRequestHandler<ReleaseCommand, TransitionResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectAuthorizationService _auth;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ReleaseCommandHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    /// <param name="currentUser">Сервис для получения данных текущего пользователя.</param>
    /// <param name="auth">Сервис проверки прав доступа к проекту.</param>
    public ReleaseCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _currentUser = currentUser;
        _auth = auth;
    }

    /// <summary>
    /// Обрабатывает команду перевода проекта в стадию релиза.
    /// </summary>
    /// <param name="request">Команда с идентификатором проекта.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="TransitionResult"/> с новой стадией <c>Support</c> при успехе,
    /// либо с описанием ошибки если переход невозможен из текущей стадии.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Выбрасывается, если текущий пользователь не аутентифицирован
    /// или не обладает ролью <c>ProjectManager</c> в данном проекте.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если проект не найден.
    /// </exception>
    public async Task<TransitionResult> Handle(ReleaseCommand request, CancellationToken ct)
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

        var result = project.Release();

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