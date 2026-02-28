using MediatR;

namespace Application.Features.Auth.Commands.Register;

/// <summary>
/// Команда для регистрации нового пользователя в системе.
/// </summary>
/// <param name="FirstName">Имя регистрируемого пользователя.</param>
/// <param name="LastName">Фамилия регистрируемого пользователя.</param>
/// <param name="Email">Адрес электронной почты, используемый для входа в систему.</param>
/// <param name="Password">Пароль в открытом виде.</param>
public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password
) : IRequest<Guid>;