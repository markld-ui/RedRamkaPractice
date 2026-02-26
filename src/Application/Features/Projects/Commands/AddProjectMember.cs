using Application.Common.Constants;
using Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Commands;

public record AddProjectMemberCommand(
    Guid ProjectId,
    Guid UserId,
    string RoleName) : IRequest;

public class AddProjectMemberCommandValidator : AbstractValidator<AddProjectMemberCommand>
{
    private static readonly string[] AllowedRoles =
    [
        RoleConstants.Developer,
        RoleConstants.Tester,
        RoleConstants.ProductManager,
        RoleConstants.DevOps,
    ];

    public AddProjectMemberCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty).WithMessage("Invalid user ID.");

        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Role is required.")
            .Must(r => AllowedRoles.Contains(r))
            .WithMessage($"Role must be one of: {string.Join(", ", AllowedRoles)}.");
    }
}

public class AddProjectMemberCommandHandler : IRequestHandler<AddProjectMemberCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectAuthorizationService _auth;

    public AddProjectMemberCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _currentUser = currentUser;
        _auth = auth;
    }

    public async Task Handle(AddProjectMemberCommand request, CancellationToken ct)
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

        var userExists = await _context.Users
            .AnyAsync(u => u.Id == request.UserId, ct);

        if (!userExists)
            throw new InvalidOperationException($"User {request.UserId} not found.");

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == request.RoleName, ct);

        if (role is null)
            throw new InvalidOperationException($"Role '{request.RoleName}' not found.");

        project.AddMember(request.UserId, role.Id);
        await _context.SaveChangesAsync(ct);
    }
}