using Domain.Models;
using MediatR;
using Application.Common.Interfaces;
using Application.Features.Auth.DTO;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : 
    IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokenService;
    
    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher hasher,
        ITokenService tokenService)
    {
        _context = context;
        _hasher = hasher;
        _tokenService = tokenService;
    }
    
    public async Task<AuthResponse> Handle(
        LoginCommand request, 
        CancellationToken ct)
    {
        var credentials = await _context.Credentials
            .Include(x => x.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(x => x.Email == request.Email, ct);

        if (credentials == null ||
            !_hasher.Verify(request.Password, credentials.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        var accessToken =
            _tokenService.GenerateAccessToken(credentials.User);

        var refreshToken = Guid.NewGuid().ToString();
        credentials.RefreshToken = refreshToken;

        await _context.SaveChangesAsync(ct);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
}