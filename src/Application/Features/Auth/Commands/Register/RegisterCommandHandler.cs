using Application.Common.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _hasher;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    public async Task<Guid> Handle(
        RegisterCommand request,
        CancellationToken ct)
    {
        var exist = await _context.Credentials
            .AnyAsync(x => x.Email == request.Email, ct);

        if (exist)
            throw new Exception("Email already exists");

        var user = new User
        {
            FirstName = request.FirstName,
            LastName =  request.LastName
        };

        var credentials = new Domain.Models.Credentials
        {
            Email =  request.Email,
            PasswordHash = _hasher.Hash(request.Password),
            User = user
        };

        user.Credentials = credentials;

        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
        
        return user.Id;
    }
}