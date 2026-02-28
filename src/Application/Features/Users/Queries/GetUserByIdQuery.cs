using Application.Common.Interfaces;
using Application.Features.Users.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Queries;

/// <summary>
/// Запрос для получения пользователя по его идентификатору.
/// </summary>
/// <param name="Id">Уникальный идентификатор пользователя.</param>
public record GetUserByIdQuery(Guid Id) : IRequest<UserDto>;

/// <summary>
/// Обработчик запроса <see cref="GetUserByIdQuery"/>.
/// </summary>
public class GetUserByIdQueryHandler
    : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="GetUserByIdQueryHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    public GetUserByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Обрабатывает запрос на получение пользователя по идентификатору.
    /// </summary>
    /// <param name="request">Запрос с идентификатором пользователя.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="UserDto"/> с данными найденного пользователя,
    /// включая его учётные данные и назначенные роли.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если пользователь с указанным идентификатором не найден.
    /// </exception>
    public async Task<UserDto> Handle(
        GetUserByIdQuery request,
        CancellationToken ct)
    {
        var user = await _context.Users
            .Include(x => x.Credentials)
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstAsync(x => x.Id == request.Id, ct);

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