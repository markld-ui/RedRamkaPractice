using Application.Common.Constants;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

/// <summary>
/// Сервис для проверки прав доступа текущего пользователя к проекту.
/// </summary>
/// <remarks>
/// Пользователи с ролью <c>Admin</c> проходят любую проверку прав без ограничений.
/// </remarks>
public class ProjectAuthorizationService : IProjectAuthorizationService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ProjectAuthorizationService"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    /// <param name="currentUser">Сервис для получения данных текущего пользователя.</param>
    public ProjectAuthorizationService(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Проверяет, является ли текущий пользователь администратором системы.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> если текущий пользователь обладает ролью <c>Admin</c>;
    /// иначе <see langword="false"/>.
    /// </returns>
    public async Task<bool> IsAdminAsync()
        => await _currentUser.IsInRoleAsync(RoleConstants.Admin);

    /// <summary>
    /// Проверяет, является ли текущий пользователь участником указанного проекта.
    /// </summary>
    /// <param name="projectId">Уникальный идентификатор проекта.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <exception cref="UnauthorizedAccessException">
    /// Выбрасывается, если текущий пользователь не является участником проекта.
    /// </exception>
    public async Task RequireProjectMemberAsync(Guid projectId, CancellationToken ct)
    {
        if (await IsAdminAsync()) return;

        var userId = _currentUser.UserId!.Value;
        var isMember = await _context.ProjectMembers
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId, ct);

        if (!isMember)
            throw new UnauthorizedAccessException(
                "You are not a member of this project.");
    }

    /// <summary>
    /// Проверяет, обладает ли текущий пользователь одной из допустимых ролей в указанном проекте.
    /// </summary>
    /// <param name="projectId">Уникальный идентификатор проекта.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <param name="allowedRoles">Массив названий ролей, которым разрешён доступ.</param>
    /// <exception cref="UnauthorizedAccessException">
    /// Выбрасывается, если текущий пользователь не является участником проекта
    /// либо его роль не входит в список допустимых.
    /// </exception>
    public async Task RequireProjectRoleAsync(
        Guid projectId,
        CancellationToken ct,
        params string[] allowedRoles)
    {
        if (await IsAdminAsync()) return;

        var userId = _currentUser.UserId!.Value;

        var member = await _context.ProjectMembers
            .Where(m => m.ProjectId == projectId && m.UserId == userId)
            .Join(_context.Roles,
                m => m.RoleId,
                r => r.Id,
                (m, r) => new { r.Name })
            .FirstOrDefaultAsync(ct);

        if (member is null)
            throw new UnauthorizedAccessException(
                "You are not a member of this project.");

        if (!allowedRoles.Contains(member.Name))
            throw new UnauthorizedAccessException(
                $"This action requires one of the following roles: {string.Join(", ", allowedRoles)}.");
    }
}