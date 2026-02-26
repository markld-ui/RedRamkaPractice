using Application.Common.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Roles.Commands;

public class AssignRoleToUserCommandHandler
    : IRequestHandler<AssignRoleToUserCommand>
{
    private readonly IApplicationDbContext _context;

    public AssignRoleToUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        AssignRoleToUserCommand request,
        CancellationToken ct)
    {
        var exists = await _context.UserRoles
            .AnyAsync(x => x.UserId == request.UserId &&
                           x.RoleId == request.RoleId, ct);

        if (exists)
            return;

        var userRole = new UserRole
        {
            UserId = request.UserId,
            RoleId = request.RoleId
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync(ct);
    }
}

public record  AssignRoleToUserCommand(
    Guid UserId,
    Guid RoleId) : IRequest;
