using Application.Common.Constants;
using Application.Common.Interfaces;
using Application.Features.Projects.Commands.Shared;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Commands;

public record ArchiveCommand(Guid ProjectId, string Reason) : IRequest<TransitionResult>;

public class ArchiveCommandValidator : AbstractValidator<ArchiveCommand>
{
    public ArchiveCommandValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required when archiving a project.")
            .MaximumLength(1000).WithMessage("Reason cannot exceed 1000 characters.");
    }
}

public class ArchiveCommandHandler : IRequestHandler<ArchiveCommand, TransitionResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectAuthorizationService _auth;

    public ArchiveCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _currentUser = currentUser;
        _auth = auth;
    }

    public async Task<TransitionResult> Handle(ArchiveCommand request, CancellationToken ct)
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

        var result = project.Archive(request.Reason);

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