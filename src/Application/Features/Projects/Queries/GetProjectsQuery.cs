using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Queries;

public record GetProjectsQuery : IRequest<IEnumerable<GetProjectDto>>;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, IEnumerable<GetProjectDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectAuthorizationService _auth;

    public GetProjectsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _currentUser = currentUser;
        _auth = auth;
    }

    public async Task<IEnumerable<GetProjectDto>> Handle(GetProjectsQuery request, CancellationToken ct)
    {
        var isAdmin = await _auth.IsAdminAsync();
        var userId = _currentUser.UserId!.Value;

        var query = _context.Projects
            .Include(p => p.Members)
            .AsNoTracking();

        // Admin видит все проекты, остальные — только свои
        if (!isAdmin)
            query = query.Where(p => p.Members.Any(m => m.UserId == userId));

        var projects = await query.ToListAsync(ct);

        return projects.Select(p => new GetProjectDto(
            p.Id,
            p.Name,
            p.Description,
            p.Stage.ToString(),
            p.CreatedAt,
            p.UpdatedAt,
            p.Members.Select(m => m.UserId)));
    }
}