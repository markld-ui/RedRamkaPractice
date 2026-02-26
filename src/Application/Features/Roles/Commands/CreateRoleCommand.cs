using Application.Common.Interfaces;
using Domain.Models;
using MediatR;

namespace Application.Features.Roles.Commands;

public class CreateRoleCommandHandler
    : IRequestHandler<CreateRoleCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(
        CreateRoleCommand request,
        CancellationToken ct)
    {
        var role = new Role
        {
            Name = request.Name,
            Description = request.Description
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(ct);

        return role.Id;
    }
}

public record CreateRoleCommand(
    string Name,
    string Description
) : IRequest<Guid>;