using Application.Common.Constants;
using Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Specifications.Commands;

public record CreateSpecificationCommand(
    Guid ProjectId,
    string Content) : IRequest<CreateSpecificationResult>;

public record CreateSpecificationResult(
    Guid Id,
    int Version,
    bool IsApproved,
    DateTime CreatedAt);

public class CreateSpecificationCommandValidator : AbstractValidator<CreateSpecificationCommand>
{
    public CreateSpecificationCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Specification content is required.")
            .MaximumLength(10000).WithMessage("Specification content cannot exceed 10000 characters.");
    }
}

public class CreateSpecificationCommandHandler
    : IRequestHandler<CreateSpecificationCommand, CreateSpecificationResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectAuthorizationService _auth;

    public CreateSpecificationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _currentUser = currentUser;
        _auth = auth;
    }

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