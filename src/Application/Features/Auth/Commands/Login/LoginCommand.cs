using MediatR;
using Application.Features.Auth.DTO;

namespace Application.Features.Auth.Commands.Login;

/// <summary>
/// Команда для аутентификации пользователя по электронной почте и паролю.
/// </summary>
/// <param name="Email">Адрес электронной почты пользователя.</param>
/// <param name="Password">Пароль пользователя в открытом виде.</param>
public record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResponse>;