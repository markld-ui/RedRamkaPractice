using Application.Common.Constants;
using Application.Common.Interfaces;
using Application.Features.Projects.Commands.Shared;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Commands;

/// <summary>
/// Команда для возврата проекта на стадию проектирования с указанием причины.
/// </summary>
/// <param name="ProjectId">Уникальный идентификатор проекта.</param>
/// <param name="Reason">Причина возврата на стадию проектирования.</param>
public record ReturnToDesignCommand(Guid ProjectId, string Reason) : IRequest<TransitionResult>;

/// <summary>
/// Валидатор команды <see cref="ReturnToDesignCommand"/>.
/// </summary>
public class ReturnToDesignCommandValidator : AbstractValidator<ReturnToDesignCommand>
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ReturnToDesignCommandValidator"/>
    /// и настраивает правила валидации.
    /// </summary>
    /// <remarks>
    /// Причина возврата обязательна и не может превышать 1000 символов.
    /// </remarks>
    public ReturnToDesignCommandValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required when returning to design.")
            .MaximumLength(1000).WithMessage("Reason cannot exceed 1000 characters.");
    }
}

/// <summary>
/// Обработчик команды <see cref="ReturnToDesignCommand"/>.
/// </summary>
public class ReturnToDesignCommandHandler : IRequestHandler<ReturnToDesignCommand, TransitionResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectAuthorizationService _auth;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ReturnToDesignCommandHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    /// <param name="currentUser">Сервис для получения данных текущего пользователя.</param>
    /// <param name="auth">Сервис проверки прав доступа к проекту.</param>
    public ReturnToDesignCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _currentUser = currentUser;
        _auth = auth;
    }

    /// <summary>
    /// Обрабатывает команду возврата проекта на стадию проектирования.
    /// </summary>
    /// <param name="request">Команда с идентификатором проекта и причиной возврата.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="TransitionResult"/> с новой стадией <c>Design</c> при успехе,
    /// либо с описанием ошибки если переход невозможен из текущей стадии.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Выбрасывается, если текущий пользователь не аутентифицирован
    /// или не обладает ролью <c>ProjectManager</c> в данном проекте.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если проект не найден.
    /// </exception>
    public async Task<TransitionResult> Handle(ReturnToDesignCommand request, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated.");

        await _auth.RequireProjectRoleAsync(
            request.ProjectId, ct,
            RoleConstants.ProjectManager);

        var project = await _context.Projects
            .Include(p => p.Transitions)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, ct);

        if (project is null)
            throw new InvalidOperationException($"Project {request.ProjectId} not found.");

        var result = project.ReturnToDesign(request.Reason);

        if (!result.IsSuccess)
            return new TransitionResult(false, result.Error, null);

        var newTransition = project.Transitions
            .OrderByDescending(t => t.ChangedAt)
            .First();
        _context.ProjectTransitions.Add(newTransition);

        await _context.SaveChangesAsync(ct);
        return new TransitionResult(true, null, result.NewStage!.Value.ToString());
    }
}