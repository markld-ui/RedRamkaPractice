using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Queries;

/// <summary>
/// Запрос для получения списка проектов, доступных текущему пользователю.
/// </summary>
/// <remarks>
/// Администратор получает полный список проектов.
/// Остальные пользователи — только проекты, участниками которых они являются.
/// </remarks>
public record GetProjectsQuery : IRequest<IEnumerable<GetProjectDto>>;

/// <summary>
/// Обработчик запроса <see cref="GetProjectsQuery"/>.
/// </summary>
public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, IEnumerable<GetProjectDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectAuthorizationService _auth;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="GetProjectsQueryHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    /// <param name="currentUser">Сервис для получения данных текущего пользователя.</param>
    /// <param name="auth">Сервис проверки прав доступа к проекту.</param>
    public GetProjectsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _currentUser = currentUser;
        _auth = auth;
    }

    /// <summary>
    /// Обрабатывает запрос на получение списка проектов.
    /// </summary>
    /// <param name="request">Запрос без дополнительных параметров.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// Коллекция <see cref="GetProjectDto"/> с проектами, доступными текущему пользователю.
    /// </returns>
    public async Task<IEnumerable<GetProjectDto>> Handle(GetProjectsQuery request, CancellationToken ct)
    {
        var isAdmin = await _auth.IsAdminAsync();
        var userId = _currentUser.UserId!.Value;

        var query = _context.Projects
            .Include(p => p.Members)
            .AsNoTracking();

        if (!isAdmin)
            query = query.Where(p => p.Members.Any(m => m.UserId == userId));

        var projects = await query.ToListAsync(ct);

        return projects.Select(p => new GetProjectDto(
            p.Id,
            p.Name,
            p.Description,
            p.Stage.ToString(),
            p.CreatedAt,
            p.UpdatedAt,
            p.Members.Select(m => m.UserId)));
    }
}