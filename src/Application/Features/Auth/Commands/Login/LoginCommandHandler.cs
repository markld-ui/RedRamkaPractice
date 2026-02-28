using Domain.Models;
using MediatR;
using Application.Common.Interfaces;
using Application.Features.Auth.DTO;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.Login;

/// <summary>
/// Обработчик команды <see cref="LoginCommand"/>.
/// </summary>
public class LoginCommandHandler :
    IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokenService;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="LoginCommandHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    /// <param name="hasher">Сервис проверки хэша пароля.</param>
    /// <param name="tokenService">Сервис генерации токенов доступа.</param>
    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher hasher,
        ITokenService tokenService)
    {
        _context = context;
        _hasher = hasher;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Обрабатывает команду аутентификации пользователя.
    /// </summary>
    /// <param name="request">Команда, содержащая электронную почту и пароль.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="AuthResponse"/> с новыми токенами доступа и обновления.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Выбрасывается, если пользователь с указанной электронной почтой не найден
    /// или пароль не совпадает с сохранённым хэшем.
    /// </exception>
    public async Task<AuthResponse> Handle(
        LoginCommand request,
        CancellationToken ct)
    {
        var credentials = await _context.Credentials
            .Include(x => x.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(x => x.Email == request.Email, ct);

        if (credentials == null ||
            !_hasher.Verify(request.Password, credentials.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        var accessToken =
            _tokenService.GenerateAccessToken(credentials.User);

        var refreshToken = Guid.NewGuid().ToString();
        credentials.RefreshToken = refreshToken;

        await _context.SaveChangesAsync(ct);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
}