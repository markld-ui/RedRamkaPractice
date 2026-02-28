using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Specifications.Queries;

/// <summary>
/// Запрос для получения спецификации проекта по её идентификатору.
/// </summary>
/// <param name="ProjectId">Уникальный идентификатор проекта.</param>
/// <param name="SpecificationId">Уникальный идентификатор спецификации.</param>
public record GetSpecificationByIdQuery(
    Guid ProjectId,
    Guid SpecificationId) : IRequest<SpecificationDto?>;

/// <summary>
/// Обработчик запроса <see cref="GetSpecificationByIdQuery"/>.
/// </summary>
public class GetSpecificationByIdQueryHandler
    : IRequestHandler<GetSpecificationByIdQuery, SpecificationDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IProjectAuthorizationService _auth;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="GetSpecificationByIdQueryHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    /// <param name="auth">Сервис проверки прав доступа к проекту.</param>
    public GetSpecificationByIdQueryHandler(
        IApplicationDbContext context,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _auth = auth;
    }

    /// <summary>
    /// Обрабатывает запрос на получение спецификации по идентификатору.
    /// </summary>
    /// <param name="request">Запрос с идентификаторами проекта и спецификации.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="SpecificationDto"/> с данными спецификации,
    /// либо <see langword="null"/> если спецификация не найдена.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Выбрасывается, если текущий пользователь не является участником проекта.
    /// </exception>
    /// <remarks>
    /// При поиске спецификации дополнительно проверяется её принадлежность
    /// к указанному проекту, что исключает возможность получения чужих спецификаций
    /// по прямому идентификатору.
    /// </remarks>
    public async Task<SpecificationDto?> Handle(
        GetSpecificationByIdQuery request,
        CancellationToken ct)
    {
        await _auth.RequireProjectMemberAsync(request.ProjectId, ct);

        var spec = await _context.ProjectSpecifications
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.Id == request.SpecificationId &&
                s.ProjectId == request.ProjectId, ct);

        if (spec is null) return null;

        return new SpecificationDto(
            spec.Id,
            spec.Version,
            spec.Content,
            spec.IsApproved,
            spec.CreatedAt,
            spec.IsApproved ? spec.ApprovedAt : null);
    }
}