using Application.Common.Constants;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class ProjectAuthorizationService : IProjectAuthorizationService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ProjectAuthorizationService(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<bool> IsAdminAsync()
        => await _currentUser.IsInRoleAsync(RoleConstants.Admin);

    public async Task RequireProjectMemberAsync(Guid projectId, CancellationToken ct)
    {
        if (await IsAdminAsync()) return;

        var userId = _currentUser.UserId!.Value;
        var isMember = await _context.ProjectMembers
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId, ct);

        if (!isMember)
            throw new UnauthorizedAccessException(
                "You are not a member of this project.");
    }

    public async Task RequireProjectRoleAsync(
        Guid projectId,
        CancellationToken ct,
        params string[] allowedRoles)
    {
        if (await IsAdminAsync()) return;

        var userId = _currentUser.UserId!.Value;

        var member = await _context.ProjectMembers
            .Where(m => m.ProjectId == projectId && m.UserId == userId)
            .Join(_context.Roles,
                m => m.RoleId,
                r => r.Id,
                (m, r) => new { r.Name })
            .FirstOrDefaultAsync(ct);

        if (member is null)
            throw new UnauthorizedAccessException(
                "You are not a member of this project.");

        if (!allowedRoles.Contains(member.Name))
            throw new UnauthorizedAccessException(
                $"This action requires one of the following roles: {string.Join(", ", allowedRoles)}.");
    }
}