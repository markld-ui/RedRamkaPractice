using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Roles.Commands;

public class DeleteRoleCommandHandler
    : IRequestHandler<DeleteRoleCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        DeleteRoleCommand request,
        CancellationToken ct)
    {
        var role = await _context.Roles
            .FirstAsync(x => x.Id == request.Id, ct);

        _context.Roles.Remove(role);

        await _context.SaveChangesAsync(ct);
    }
}


public record DeleteRoleCommand(Guid Id) : IRequest;

