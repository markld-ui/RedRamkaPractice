using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Credentials.Commands;

/// <summary>
/// Команда для отзыва refresh-токена указанного пользователя.
/// </summary>
/// <param name="UserId">Уникальный идентификатор пользователя, чей токен необходимо отозвать.</param>
public record RevokeRefreshTokenCommand(Guid UserId) : IRequest;

/// <summary>
/// Обработчик команды <see cref="RevokeRefreshTokenCommand"/>.
/// </summary>
public class RevokeRefreshTokenCommandHandler
    : IRequestHandler<RevokeRefreshTokenCommand>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="RevokeRefreshTokenCommandHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    public RevokeRefreshTokenCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Обрабатывает команду отзыва refresh-токена пользователя.
    /// </summary>
    /// <param name="request">Команда, содержащая идентификатор пользователя.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <remarks>
    /// После выполнения команды пользователь не сможет обновить токен доступа
    /// и будет вынужден пройти повторную аутентификацию.
    /// </remarks>
    public async Task Handle(
        RevokeRefreshTokenCommand request,
        CancellationToken ct)
    {
        var credentials = await _context.Credentials
            .FirstAsync(x => x.UserId == request.UserId, ct);

        credentials.RefreshToken = null;

        await _context.SaveChangesAsync(ct);
    }
}