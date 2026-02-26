using Application.Common.Constants;
using Application.Common.Interfaces;
using Application.Features.Projects.Commands.Shared;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Commands;

public record ReturnToDesignCommand(Guid ProjectId, string Reason) : IRequest<TransitionResult>;

public class ReturnToDesignCommandValidator : AbstractValidator<ReturnToDesignCommand>
{
    public ReturnToDesignCommandValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required when returning to design.")
            .MaximumLength(1000).WithMessage("Reason cannot exceed 1000 characters.");
    }
}

public class ReturnToDesignCommandHandler : IRequestHandler<ReturnToDesignCommand, TransitionResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectAuthorizationService _auth;

    public ReturnToDesignCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _currentUser = currentUser;
        _auth = auth;
    }

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