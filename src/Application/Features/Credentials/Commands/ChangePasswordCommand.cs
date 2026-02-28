using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Credentials.Commands;

/// <summary>
/// Команда для смены пароля текущего аутентифицированного пользователя.
/// </summary>
/// <param name="CurrentPassword">Текущий пароль в открытом виде.</param>
/// <param name="NewPassword">Новый пароль в открытом виде.</param>
public record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword) : IRequest;

/// <summary>
/// Обработчик команды <see cref="ChangePasswordCommand"/>.
/// </summary>
public class ChangePasswordCommandHandler
    : IRequestHandler<ChangePasswordCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _hasher;
    private readonly ICurrentUserService _currentUser;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ChangePasswordCommandHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    /// <param name="hasher">Сервис хэширования и проверки паролей.</param>
    /// <param name="currentUser">Сервис для получения данных текущего пользователя.</param>
    public ChangePasswordCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher hasher,
        ICurrentUserService currentUser)
    {
        _context = context;
        _hasher = hasher;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Обрабатывает команду смены пароля.
    /// </summary>
    /// <param name="request">Команда, содержащая текущий и новый пароли.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <exception cref="UnauthorizedAccessException">
    /// Выбрасывается, если пользователь не аутентифицирован
    /// или текущий пароль не совпадает с сохранённым хэшем.
    /// </exception>
    /// <remarks>
    /// После успешной смены пароля refresh-токен пользователя аннулируется,
    /// что требует повторной аутентификации.
    /// </remarks>
    public async Task Handle(
        ChangePasswordCommand request,
        CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var credentials = await _context.Credentials
            .FirstAsync(x => x.UserId == _currentUser.UserId, ct);

        if (!_hasher.Verify(
                request.CurrentPassword,
                credentials.PasswordHash))
            throw new UnauthorizedAccessException();

        credentials.PasswordHash =
            _hasher.Hash(request.NewPassword);

        credentials.RefreshToken = null;

        await _context.SaveChangesAsync(ct);
    }
}