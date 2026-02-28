using Application.Common.Constants;
using Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Specifications.Commands;

/// <summary>
/// Команда для создания новой спецификации проекта.
/// </summary>
/// <param name="ProjectId">Уникальный идентификатор проекта.</param>
/// <param name="Content">Содержимое спецификации.</param>
public record CreateSpecificationCommand(
    Guid ProjectId,
    string Content) : IRequest<CreateSpecificationResult>;

/// <summary>
/// Результат создания спецификации проекта.
/// </summary>
/// <param name="Id">Уникальный идентификатор созданной спецификации.</param>
/// <param name="Version">Автоматически присвоенный номер версии.</param>
/// <param name="IsApproved">Признак утверждённости спецификации. При создании всегда <see langword="false"/>.</param>
/// <param name="CreatedAt">Дата и время создания спецификации в формате UTC.</param>
public record CreateSpecificationResult(
    Guid Id,
    int Version,
    bool IsApproved,
    DateTime CreatedAt);

/// <summary>
/// Валидатор команды <see cref="CreateSpecificationCommand"/>.
/// </summary>
public class CreateSpecificationCommandValidator : AbstractValidator<CreateSpecificationCommand>
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CreateSpecificationCommandValidator"/>
    /// и настраивает правила валидации.
    /// </summary>
    /// <remarks>
    /// Содержимое спецификации обязательно и не может превышать 10 000 символов.
    /// </remarks>
    public CreateSpecificationCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Specification content is required.")
            .MaximumLength(10000).WithMessage("Specification content cannot exceed 10000 characters.");
    }
}

/// <summary>
/// Обработчик команды <see cref="CreateSpecificationCommand"/>.
/// </summary>
public class CreateSpecificationCommandHandler
    : IRequestHandler<CreateSpecificationCommand, CreateSpecificationResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectAuthorizationService _auth;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CreateSpecificationCommandHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    /// <param name="currentUser">Сервис для получения данных текущего пользователя.</param>
    /// <param name="auth">Сервис проверки прав доступа к проекту.</param>
    public CreateSpecificationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _currentUser = currentUser;
        _auth = auth;
    }

    /// <summary>
    /// Обрабатывает команду создания спецификации проекта.
    /// </summary>
    /// <param name="request">Команда с идентификатором проекта и содержимым спецификации.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="CreateSpecificationResult"/> с данными созданной спецификации.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Выбрасывается, если текущий пользователь не аутентифицирован
    /// или не обладает ролью <c>ProjectManager</c> в данном проекте.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если проект не найден или находится в стадии <c>Archived</c>.
    /// </exception>
    /// <remarks>
    /// Номер версии спецификации присваивается автоматически как следующий
    /// за максимальным существующим в рамках проекта.
    /// </remarks>
    public async Task<CreateSpecificationResult> Handle(
        CreateSpecificationCommand request,
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

        project.AddSpecification(request.Content);

        var spec = project.Specifications
            .OrderByDescending(s => s.Version)
            .First();

        _context.ProjectSpecifications.Add(spec);

        await _context.SaveChangesAsync(ct);

        return new CreateSpecificationResult(
            spec.Id,
            spec.Version,
            spec.IsApproved,
            spec.CreatedAt);
    }
}