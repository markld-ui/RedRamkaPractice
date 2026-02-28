using Domain.Models;

namespace Application.Common.Interfaces;

/// <summary>
/// Интерфейс сервиса для получения данных о текущем аутентифицированном пользователе.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Уникальный идентификатор текущего пользователя.
    /// Равен <see langword="null"/>, если пользователь не аутентифицирован.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Адрес электронной почты текущего пользователя.
    /// Равен <see langword="null"/>, если пользователь не аутентифицирован.
    /// </summary>
    string? UserEmail { get; }

    /// <summary>
    /// Признак аутентифицированности текущего пользователя.
    /// <see langword="true"/> если пользователь аутентифицирован; иначе <see langword="false"/>.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Проверяет, принадлежит ли текущий пользователь к указанной роли.
    /// </summary>
    /// <param name="role">Название проверяемой роли.</param>
    /// <returns>
    /// <see langword="true"/> если пользователь состоит в указанной роли;
    /// иначе <see langword="false"/>.
    /// </returns>
    Task<bool> IsInRoleAsync(string role);
}