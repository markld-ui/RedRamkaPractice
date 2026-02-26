using Application.Common.Interfaces;
using Application.Features.Users.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Queries;

public class GetAllUsersQueryHandler
    : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> Handle(
        GetAllUsersQuery request,
        CancellationToken ct)
    {
        return await _context.Users
            .Include(x => x.Credentials)
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .Select(user => new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Credentials.Email,
                Roles = user.UserRoles
                    .Select(r => r.Role.Name)
                    .ToList()
            })
            .ToListAsync(ct);
    }
}

public record GetAllUsersQuery() : IRequest<List<UserDto>>;
