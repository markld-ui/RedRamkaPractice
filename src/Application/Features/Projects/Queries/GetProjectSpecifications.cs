using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Specifications.Queries;

/// <summary>
/// Запрос для получения списка всех спецификаций указанного проекта.
/// </summary>
/// <param name="ProjectId">Уникальный идентификатор проекта.</param>
public record GetProjectSpecificationsQuery(Guid ProjectId)
    : IRequest<IEnumerable<SpecificationDto>>;

/// <summary>
/// DTO с данными спецификации проекта, возвращаемыми в ответ на запросы.
/// </summary>
/// <param name="Id">Уникальный идентификатор спецификации.</param>
/// <param name="Version">Порядковый номер версии спецификации.</param>
/// <param name="Content">Содержимое спецификации.</param>
/// <param name="IsApproved">Признак утверждённости спецификации.</param>
/// <param name="CreatedAt">Дата и время создания спецификации в формате UTC.</param>
/// <param name="ApprovedAt">
/// Дата и время утверждения спецификации в формате UTC.
/// Равно <see langword="null"/> если спецификация не утверждена.
/// </param>
public record SpecificationDto(
    Guid Id,
    int Version,
    string Content,
    bool IsApproved,
    DateTime CreatedAt,
    DateTime? ApprovedAt);

/// <summary>
/// Обработчик запроса <see cref="GetProjectSpecificationsQuery"/>.
/// </summary>
public class GetProjectSpecificationsQueryHandler
    : IRequestHandler<GetProjectSpecificationsQuery, IEnumerable<SpecificationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IProjectAuthorizationService _auth;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="GetProjectSpecificationsQueryHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    /// <param name="auth">Сервис проверки прав доступа к проекту.</param>
    public GetProjectSpecificationsQueryHandler(
        IApplicationDbContext context,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _auth = auth;
    }

    /// <summary>
    /// Обрабатывает запрос на получение спецификаций проекта.
    /// </summary>
    /// <param name="request">Запрос с идентификатором проекта.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// Коллекция <see cref="SpecificationDto"/>, отсортированная по номеру версии по возрастанию.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Выбрасывается, если текущий пользователь не является участником проекта.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если проект не найден.
    /// </exception>
    public async Task<IEnumerable<SpecificationDto>> Handle(
        GetProjectSpecificationsQuery request,
        CancellationToken ct)
    {
        await _auth.RequireProjectMemberAsync(request.ProjectId, ct);

        var projectExists = await _context.Projects
            .AnyAsync(p => p.Id == request.ProjectId, ct);

        if (!projectExists)
            throw new InvalidOperationException($"Project {request.ProjectId} not found.");

        var specs = await _context.ProjectSpecifications
            .Where(s => s.ProjectId == request.ProjectId)
            .OrderBy(s => s.Version)
            .AsNoTracking()
            .ToListAsync(ct);

        return specs.Select(s => new SpecificationDto(
            s.Id,
            s.Version,
            s.Content,
            s.IsApproved,
            s.CreatedAt,
            s.IsApproved ? s.ApprovedAt : null));
    }
}