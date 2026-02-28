using Application.Common.Constants;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Commands;

/// <summary>
/// Команда для удаления участника из проекта.
/// </summary>
/// <param name="ProjectId">Уникальный идентификатор проекта.</param>
/// <param name="UserId">Уникальный идентификатор удаляемого участника.</param>
public record RemoveProjectMemberCommand(Guid ProjectId, Guid UserId) : IRequest;

/// <summary>
/// Обработчик команды <see cref="RemoveProjectMemberCommand"/>.
/// </summary>
public class RemoveProjectMemberCommandHandler : IRequestHandler<RemoveProjectMemberCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectAuthorizationService _auth;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="RemoveProjectMemberCommandHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    /// <param name="currentUser">Сервис для получения данных текущего пользователя.</param>
    /// <param name="auth">Сервис проверки прав доступа к проекту.</param>
    public RemoveProjectMemberCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _currentUser = currentUser;
        _auth = auth;
    }

    /// <summary>
    /// Обрабатывает команду удаления участника из проекта.
    /// </summary>
    /// <param name="request">Команда с идентификаторами проекта и удаляемого участника.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <exception cref="UnauthorizedAccessException">
    /// Выбрасывается, если текущий пользователь не аутентифицирован
    /// или не обладает ролью <c>ProjectManager</c> в данном проекте.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается в следующих случаях:
    /// <list type="bullet">
    ///   <item>Проект не найден.</item>
    ///   <item>Указанный пользователь не является участником проекта.</item>
    ///   <item>Удаляемый участник является единственным <c>ProjectManager</c> в проекте.</item>
    /// </list>
    /// </exception>
    public async Task Handle(RemoveProjectMemberCommand request, CancellationToken ct)
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

        var member = project.Members.FirstOrDefault(m => m.UserId == request.UserId);

        if (member is null)
            throw new InvalidOperationException("User is not a member of this project.");

        var managerRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == RoleConstants.ProjectManager, ct);

        if (managerRole is not null && member.RoleId == managerRole.Id)
        {
            var managerCount = project.Members.Count(m => m.RoleId == managerRole.Id);
            if (managerCount <= 1)
                throw new InvalidOperationException("Cannot remove the last ProjectManager.");
        }

        _context.ProjectMembers.Remove(member);
        await _context.SaveChangesAsync(ct);
    }
}