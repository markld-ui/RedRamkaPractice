namespace Application.Common.Interfaces;

/// <summary>
/// Проверяет права текущего пользователя относительно конкретного проекта.
/// Admin проходит любую проверку вне зависимости от членства.
/// </summary>
public interface IProjectAuthorizationService
{
    /// <summary>
    /// Бросает UnauthorizedAccessException если у пользователя нет
    /// ни одной из указанных ролей в проекте (и он не Admin).
    /// </summary>
    Task RequireProjectRoleAsync(
        Guid projectId,
        CancellationToken ct,
        params string[] allowedRoles);

    /// <summary>
    /// Проверяет, является ли пользователь членом проекта (любая роль) или Admin.
    /// </summary>
    Task RequireProjectMemberAsync(Guid projectId, CancellationToken ct);

    /// <summary>
    /// Возвращает true если пользователь — Admin на уровне системы.
    /// </summary>
    Task<bool> IsAdminAsync();
}