using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Queries;

public record GetProjectByIdQuery(Guid Id) : IRequest<GetProjectDto?>;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, GetProjectDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IProjectAuthorizationService _auth;

    public GetProjectByIdQueryHandler(
        IApplicationDbContext context,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _auth = auth;
    }

    public async Task<GetProjectDto?> Handle(GetProjectByIdQuery request, CancellationToken ct)
    {
        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

        if (project is null) return null;

        // Бросит UnauthorizedAccessException если нет доступа
        await _auth.RequireProjectMemberAsync(request.Id, ct);

        return new GetProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.Stage.ToString(),
            project.CreatedAt,
            project.UpdatedAt,
            project.Members.Select(m => m.UserId));
    }
}

public record GetProjectDto(
    Guid Id,
    string Name,
    string Description,
    string Stage,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IEnumerable<Guid> MemberIds);