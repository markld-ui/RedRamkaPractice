using Application.Common.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Roles.Queries;

public class GetAllRolesQueryHandler
    : IRequestHandler<GetAllRolesQuery, List<Role>>
{
    private readonly IApplicationDbContext _context;

    public GetAllRolesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Role>> Handle(
        GetAllRolesQuery request,
        CancellationToken ct)
    {
        return await _context.Roles.ToListAsync(ct);
    }
}


public record GetAllRolesQuery() : IRequest<List<Role>>;

