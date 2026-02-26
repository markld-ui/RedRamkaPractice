using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Users.Commands;

public class DeleteUserCommandHandler
    : IRequestHandler<DeleteUserCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        DeleteUserCommand request,
        CancellationToken ct)
    {
        var user = await _context.Users
            .FirstAsync(x => x.Id == request.Id, ct);

        _context.Users.Remove(user);

        await _context.SaveChangesAsync(ct);
    }
}

public record DeleteUserCommand(Guid Id) : IRequest;
