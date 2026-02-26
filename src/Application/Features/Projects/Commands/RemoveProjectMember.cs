using Application.Common.Constants;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Commands;

public record RemoveProjectMemberCommand(Guid ProjectId, Guid UserId) : IRequest;

public class RemoveProjectMemberCommandHandler : IRequestHandler<RemoveProjectMemberCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectAuthorizationService _auth;

    public RemoveProjectMemberCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _currentUser = currentUser;
        _auth = auth;
    }

    public async Task Handle(RemoveProjectMemberCommand request, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated.");

        await _auth.RequireProjectRoleAsync(
            request.ProjectId, ct,
            RoleConstants.ProjectManager);

        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, ct);

        if (project is null)
            throw new InvalidOperationException($"Project {request.ProjectId} not found.");

        var member = project.Members.FirstOrDefault(m => m.UserId == request.UserId);

        if (member is null)
            throw new InvalidOperationException("User is not a member of this project.");

        var managerRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == RoleConstants.ProjectManager, ct);

        if (managerRole is not null && member.RoleId == managerRole.Id)
        {
            var managerCount = project.Members.Count(m => m.RoleId == managerRole.Id);
            if (managerCount <= 1)
                throw new InvalidOperationException("Cannot remove the last ProjectManager.");
        }

        _context.ProjectMembers.Remove(member);
        await _context.SaveChangesAsync(ct);
    }
}