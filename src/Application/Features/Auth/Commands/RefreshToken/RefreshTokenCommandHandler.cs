using Application.Common.Interfaces;
using Application.Features.Auth.DTO;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Обработчик команды <see cref="RefreshTokenCommand"/>.
/// </summary>
public class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="RefreshTokenCommandHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    /// <param name="tokenService">Сервис генерации токенов доступа.</param>
    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Обрабатывает команду обновления токена доступа.
    /// </summary>
    /// <param name="request">Команда, содержащая действующий refresh-токен.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="AuthResponse"/> с новой парой токенов доступа и обновления.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Выбрасывается, если переданный refresh-токен не найден в базе данных
    /// или уже был отозван.
    /// </exception>
    public async Task<AuthResponse> Handle(
        RefreshTokenCommand request,
        CancellationToken ct)
    {
        var credentials = await _context.Credentials
            .Include(x => x.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(
                x => x.RefreshToken == request.RefreshToken, ct);

        if (credentials == null)
            throw new UnauthorizedAccessException();

        var newAccessToken =
            _tokenService.GenerateAccessToken(credentials.User);

        var newRefreshToken = Guid.NewGuid().ToString();
        credentials.RefreshToken = newRefreshToken;

        await _context.SaveChangesAsync(ct);

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
}