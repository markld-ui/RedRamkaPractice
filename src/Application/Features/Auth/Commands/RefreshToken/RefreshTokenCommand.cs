using MediatR;
using Application.Features.Auth.DTO;

namespace Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Команда для обновления токена доступа по refresh-токену.
/// </summary>
/// <param name="RefreshToken">Действующий refresh-токен пользователя.</param>
public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<AuthResponse>;