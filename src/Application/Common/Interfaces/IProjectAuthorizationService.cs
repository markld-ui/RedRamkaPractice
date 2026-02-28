namespace Application.Common.Interfaces;

/// <summary>
/// Интерфейс сервиса для проверки прав доступа текущего пользователя к проекту.
/// </summary>
/// <remarks>
/// Пользователи с ролью <c>Admin</c> проходят любую проверку прав
/// без ограничений, вне зависимости от членства в проекте.
/// </remarks>
public interface IProjectAuthorizationService
{
    /// <summary>
    /// Проверяет, обладает ли текущий пользователь одной из допустимых ролей
    /// в указанном проекте.
    /// </summary>
    /// <param name="projectId">Уникальный идентификатор проекта.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <param name="allowedRoles">Массив названий ролей, которым разрешён доступ.</param>
    /// <exception cref="UnauthorizedAccessException">
    /// Выбрасывается, если текущий пользователь не является участником проекта
    /// либо его роль не входит в список допустимых.
    /// </exception>
    Task RequireProjectRoleAsync(
        Guid projectId,
        CancellationToken ct,
        params string[] allowedRoles);

    /// <summary>
    /// Проверяет, является ли текущий пользователь участником указанного проекта
    /// в любой роли.
    /// </summary>
    /// <param name="projectId">Уникальный идентификатор проекта.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <exception cref="UnauthorizedAccessException">
    /// Выбрасывается, если текущий пользователь не является участником проекта.
    /// </exception>
    Task RequireProjectMemberAsync(Guid projectId, CancellationToken ct);

    /// <summary>
    /// Проверяет, является ли текущий пользователь администратором системы.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> если текущий пользователь обладает ролью <c>Admin</c>;
    /// иначе <see langword="false"/>.
    /// </returns>
    Task<bool> IsAdminAsync();
}