using Domain.Models;

namespace Application.Common.Interfaces;

/// <summary>
/// Интерфейс сервиса для генерации токенов доступа.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Генерирует JWT-токен доступа для указанного пользователя.
    /// </summary>
    /// <param name="user">Пользователь, для которого выпускается токен.</param>
    /// <returns>
    /// Строка с подписанным JWT-токеном доступа.
    /// </returns>
    string GenerateAccessToken(User user);
}