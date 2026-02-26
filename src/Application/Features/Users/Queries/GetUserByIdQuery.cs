using Application.Common.Interfaces;
using Application.Features.Users.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Queries;

public class GetUserByIdQueryHandler
    : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IApplicationDbContext _context;

    public GetUserByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto> Handle(
        GetUserByIdQuery request,
        CancellationToken ct)
    {
        var user = await _context.Users
            .Include(x => x.Credentials)
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstAsync(x => x.Id == request.Id, ct);

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

public record GetUserByIdQuery(Guid Id) : IRequest<UserDto>;

