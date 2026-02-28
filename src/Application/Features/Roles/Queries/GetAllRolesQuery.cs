using Application.Common.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Roles.Queries;

/// <summary>
/// Запрос для получения списка всех ролей в системе.
/// </summary>
public record GetAllRolesQuery() : IRequest<List<Role>>;

/// <summary>
/// Обработчик запроса <see cref="GetAllRolesQuery"/>.
/// </summary>
public class GetAllRolesQueryHandler
    : IRequestHandler<GetAllRolesQuery, List<Role>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="GetAllRolesQueryHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    public GetAllRolesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Обрабатывает запрос на получение всех ролей.
    /// </summary>
    /// <param name="request">Запрос без дополнительных параметров.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// Список всех <see cref="Role"/> существующих в системе.
    /// </returns>
    public async Task<List<Role>> Handle(
        GetAllRolesQuery request,
        CancellationToken ct)
    {
        return await _context.Roles.ToListAsync(ct);
    }
}