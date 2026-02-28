using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Queries;

/// <summary>
/// Запрос для получения проекта по его идентификатору.
/// </summary>
/// <param name="Id">Уникальный идентификатор проекта.</param>
public record GetProjectByIdQuery(Guid Id) : IRequest<GetProjectDto?>;

/// <summary>
/// DTO с данными проекта, возвращаемыми в ответ на запросы.
/// </summary>
/// <param name="Id">Уникальный идентификатор проекта.</param>
/// <param name="Name">Название проекта.</param>
/// <param name="Description">Описание проекта.</param>
/// <param name="Stage">Текущая стадия жизненного цикла проекта.</param>
/// <param name="CreatedAt">Дата и время создания проекта в формате UTC.</param>
/// <param name="UpdatedAt">Дата и время последнего изменения проекта в формате UTC.</param>
/// <param name="MemberIds">Коллекция идентификаторов участников проекта.</param>
public record GetProjectDto(
    Guid Id,
    string Name,
    string Description,
    string Stage,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IEnumerable<Guid> MemberIds);

/// <summary>
/// Обработчик запроса <see cref="GetProjectByIdQuery"/>.
/// </summary>
public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, GetProjectDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IProjectAuthorizationService _auth;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="GetProjectByIdQueryHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    /// <param name="auth">Сервис проверки прав доступа к проекту.</param>
    public GetProjectByIdQueryHandler(
        IApplicationDbContext context,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _auth = auth;
    }

    /// <summary>
    /// Обрабатывает запрос на получение проекта по идентификатору.
    /// </summary>
    /// <param name="request">Запрос с идентификатором проекта.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="GetProjectDto"/> с данными проекта,
    /// либо <see langword="null"/> если проект не найден.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Выбрасывается, если текущий пользователь не является участником проекта.
    /// </exception>
    public async Task<GetProjectDto?> Handle(GetProjectByIdQuery request, CancellationToken ct)
    {
        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

        if (project is null) return null;

        await _auth.RequireProjectMemberAsync(request.Id, ct);

        return new GetProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.Stage.ToString(),
            project.CreatedAt,
            project.UpdatedAt,
            project.Members.Select(m => m.UserId));
    }
}