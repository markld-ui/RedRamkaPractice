using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Credentials.Commands;

public class RevokeRefreshTokenCommandHandler
    : IRequestHandler<RevokeRefreshTokenCommand>
{
    private readonly IApplicationDbContext _context;

    public RevokeRefreshTokenCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        RevokeRefreshTokenCommand request,
        CancellationToken ct)
    {
        var credentials = await _context.Credentials
            .FirstAsync(x => x.UserId == request.UserId, ct);

        credentials.RefreshToken = null;

        await _context.SaveChangesAsync(ct);
    }
}


public record RevokeRefreshTokenCommand(Guid UserId) : IRequest;

