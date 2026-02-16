using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Queries;

public class GetProjectByIdHandler : IRequestHandler<GetProjectByIdQuery, GetProjectDto?>
{
    private readonly IApplicationDbContext _context;

    public GetProjectByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetProjectDto?> Handle(GetProjectByIdQuery request, CancellationToken ct)
    {
        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

        if (project == null) return null;

        var dto = new GetProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.Stage.ToString(),
            project.CreatedAt,
            project.UpdatedAt,
            project.Members.Select(m => m.UserId));

        return dto;
    }
}
