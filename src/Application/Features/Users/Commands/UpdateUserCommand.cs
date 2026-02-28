using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Commands;

/// <summary>
/// Команда для обновления данных текущего аутентифицированного пользователя.
/// </summary>
/// <param name="FirstName">Новое имя пользователя.</param>
/// <param name="LastName">Новая фамилия пользователя.</param>
public record UpdateUserCommand(
    string FirstName,
    string LastName
) : IRequest;

/// <summary>
/// Обработчик команды <see cref="UpdateUserCommand"/>.
/// </summary>
public class UpdateUserCommandHandler
    : IRequestHandler<UpdateUserCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="UpdateUserCommandHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    /// <param name="currentUser">Сервис для получения данных текущего пользователя.</param>
    public UpdateUserCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Обрабатывает команду обновления данных пользователя.
    /// </summary>
    /// <param name="request">Команда с новыми именем и фамилией пользователя.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <exception cref="UnauthorizedAccessException">
    /// Выбрасывается, если текущий пользователь не аутентифицирован.
    /// </exception>
    public async Task Handle(
        UpdateUserCommand request,
        CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var user = await _context.Users
            .FirstAsync(x => x.Id == _currentUser.UserId, ct);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        await _context.SaveChangesAsync(ct);
    }
}