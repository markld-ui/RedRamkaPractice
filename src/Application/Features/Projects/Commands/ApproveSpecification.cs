using Application.Common.Constants;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Specifications.Commands;

public record ApproveSpecificationCommand(
    Guid ProjectId,
    Guid SpecificationId) : IRequest<ApproveSpecificationResult>;

public record ApproveSpecificationResult(
    Guid Id,
    int Version,
    bool IsApproved,
    DateTime ApprovedAt);

public class ApproveSpecificationCommandHandler
    : IRequestHandler<ApproveSpecificationCommand, ApproveSpecificationResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectAuthorizationService _auth;

    public ApproveSpecificationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _currentUser = currentUser;
        _auth = auth;
    }

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

        // ApproveSpecification проверит принадлежность к проекту,
        // статус и отзовёт предыдущую одобренную
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