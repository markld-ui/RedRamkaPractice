using Application.Common.Interfaces;
using Application.Features.Users.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Queries;

/// <summary>
/// Запрос для получения списка всех пользователей системы.
/// </summary>
public record GetAllUsersQuery() : IRequest<List<UserDto>>;

/// <summary>
/// Обработчик запроса <see cref="GetAllUsersQuery"/>.
/// </summary>
public class GetAllUsersQueryHandler
    : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="GetAllUsersQueryHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    public GetAllUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Обрабатывает запрос на получение всех пользователей.
    /// </summary>
    /// <param name="request">Запрос без дополнительных параметров.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// Список <see cref="UserDto"/> со всеми пользователями системы,
    /// включая их учётные данные и назначенные роли.
    /// </returns>
    public async Task<List<UserDto>> Handle(
        GetAllUsersQuery request,
        CancellationToken ct)
    {
        return await _context.Users
            .Include(x => x.Credentials)
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .Select(user => new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Credentials.Email,
                Roles = user.UserRoles
                    .Select(r => r.Role.Name)
                    .ToList()
            })
            .ToListAsync(ct);
    }
}