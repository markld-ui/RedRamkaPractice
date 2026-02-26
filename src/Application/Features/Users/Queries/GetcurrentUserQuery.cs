using Application.Common.Interfaces;
using Application.Features.Users.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Queries;

public class GetCurrentUserQueryHandler
    : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetCurrentUserQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<UserDto> Handle(
        GetCurrentUserQuery request,
        CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var user = await _context.Users
            .Include(x => x.Credentials)
            .Include(x => x.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstAsync(x => x.Id == _currentUser.UserId, ct);

        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Credentials.Email,
            Roles = user.UserRoles
                .Select(r => r.Role.Name)
                .ToList()
        };
    }
}

public record GetCurrentUserQuery() : IRequest<UserDto>;