using Application.Common.Interfaces;
using Application.Features.Auth.DTO;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler 
    : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(
        RefreshTokenCommand request,
        CancellationToken ct)
    {
        var credentials = await _context.Credentials
            .Include(x => x.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(
                x => x.RefreshToken == request.RefreshToken, ct);

        if (credentials == null)
            throw new UnauthorizedAccessException();

        var newAccessToken =
            _tokenService.GenerateAccessToken(credentials.User);

        var newRefreshToken = Guid.NewGuid().ToString();
        credentials.RefreshToken = newRefreshToken;

        await _context.SaveChangesAsync(ct);

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
}