using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Specifications.Queries;

public record GetProjectSpecificationsQuery(Guid ProjectId)
    : IRequest<IEnumerable<SpecificationDto>>;

public record SpecificationDto(
    Guid Id,
    int Version,
    string Content,
    bool IsApproved,
    DateTime CreatedAt,
    DateTime? ApprovedAt);

public class GetProjectSpecificationsQueryHandler
    : IRequestHandler<GetProjectSpecificationsQuery, IEnumerable<SpecificationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IProjectAuthorizationService _auth;

    public GetProjectSpecificationsQueryHandler(
        IApplicationDbContext context,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _auth = auth;
    }

    public async Task<IEnumerable<SpecificationDto>> Handle(
        GetProjectSpecificationsQuery request,
        CancellationToken ct)
    {
        await _auth.RequireProjectMemberAsync(request.ProjectId, ct);

        var projectExists = await _context.Projects
            .AnyAsync(p => p.Id == request.ProjectId, ct);

        if (!projectExists)
            throw new InvalidOperationException($"Project {request.ProjectId} not found.");

        var specs = await _context.ProjectSpecifications
            .Where(s => s.ProjectId == request.ProjectId)
            .OrderBy(s => s.Version)
            .AsNoTracking()
            .ToListAsync(ct);

        return specs.Select(s => new SpecificationDto(
            s.Id,
            s.Version,
            s.Content,
            s.IsApproved,
            s.CreatedAt,
            s.IsApproved ? s.ApprovedAt : null));
    }
}