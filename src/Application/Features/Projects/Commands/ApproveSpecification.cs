using Application.Common.Constants;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Specifications.Commands;

/// <summary>
/// Команда для утверждения спецификации проекта.
/// </summary>
/// <param name="ProjectId">Уникальный идентификатор проекта.</param>
/// <param name="SpecificationId">Уникальный идентификатор утверждаемой спецификации.</param>
public record ApproveSpecificationCommand(
    Guid ProjectId,
    Guid SpecificationId) : IRequest<ApproveSpecificationResult>;

/// <summary>
/// Результат утверждения спецификации проекта.
/// </summary>
/// <param name="Id">Уникальный идентификатор спецификации.</param>
/// <param name="Version">Номер версии утверждённой спецификации.</param>
/// <param name="IsApproved">Признак утверждённости спецификации.</param>
/// <param name="ApprovedAt">Дата и время утверждения спецификации в формате UTC.</param>
public record ApproveSpecificationResult(
    Guid Id,
    int Version,
    bool IsApproved,
    DateTime ApprovedAt);

/// <summary>
/// Обработчик команды <see cref="ApproveSpecificationCommand"/>.
/// </summary>
public class ApproveSpecificationCommandHandler
    : IRequestHandler<ApproveSpecificationCommand, ApproveSpecificationResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectAuthorizationService _auth;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ApproveSpecificationCommandHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    /// <param name="currentUser">Сервис для получения данных текущего пользователя.</param>
    /// <param name="auth">Сервис проверки прав доступа к проекту.</param>
    public ApproveSpecificationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _currentUser = currentUser;
        _auth = auth;
    }

    /// <summary>
    /// Обрабатывает команду утверждения спецификации.
    /// </summary>
    /// <param name="request">Команда с идентификаторами проекта и спецификации.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="ApproveSpecificationResult"/> с данными утверждённой спецификации.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Выбрасывается, если текущий пользователь не аутентифицирован
    /// или не обладает ролью <c>ProjectManager</c> в данном проекте.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если проект не найден, спецификация не принадлежит проекту
    /// или уже является утверждённой.
    /// </exception>
    /// <remarks>
    /// При утверждении новой спецификации ранее утверждённая автоматически отзывается,
    /// так как актуальной может быть только одна.
    /// </remarks>
    public async Task<ApproveSpecificationResult> Handle(
        ApproveSpecificationCommand request,
        CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated.");

        await _auth.RequireProjectRoleAsync(
            request.ProjectId, ct,
            RoleConstants.ProjectManager);

        var project = await _context.Projects
            .Include(p => p.Specifications)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, ct);

        if (project is null)
            throw new InvalidOperationException($"Project {request.ProjectId} not found.");

        project.ApproveSpecification(request.SpecificationId);

        await _context.SaveChangesAsync(ct);

        var spec = project.Specifications.First(s => s.Id == request.SpecificationId);

        return new ApproveSpecificationResult(
            spec.Id,
            spec.Version,
            spec.IsApproved,
            spec.ApprovedAt);
    }
}