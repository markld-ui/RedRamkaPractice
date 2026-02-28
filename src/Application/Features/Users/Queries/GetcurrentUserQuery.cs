using Application.Common.Interfaces;
using Application.Features.Users.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Queries;

/// <summary>
/// Запрос для получения данных текущего аутентифицированного пользователя.
/// </summary>
public record GetCurrentUserQuery() : IRequest<UserDto>;

/// <summary>
/// Обработчик запроса <see cref="GetCurrentUserQuery"/>.
/// </summary>
public class GetCurrentUserQueryHandler
    : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="GetCurrentUserQueryHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    /// <param name="currentUser">Сервис для получения данных текущего пользователя.</param>
    public GetCurrentUserQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Обрабатывает запрос на получение данных текущего пользователя.
    /// </summary>
    /// <param name="request">Запрос без дополнительных параметров.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="UserDto"/> с данными текущего пользователя,
    /// включая его учётные данные и назначенные роли.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Выбрасывается, если текущий пользователь не аутентифицирован.
    /// </exception>
    public async Task<UserDto> Handle(
        GetCurrentUserQuery request,
        CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var user = await _context.Users
            .Include(x => x.Credentials)
            .Include(x => x.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstAsync(x => x.Id == _currentUser.UserId, ct);

        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Credentials.Email,
            Roles = user.UserRoles
                .Select(r => r.Role.Name)
                .ToList()
        };
    }
}