using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Specifications.Queries;

public record GetSpecificationByIdQuery(
    Guid ProjectId,
    Guid SpecificationId) : IRequest<SpecificationDto?>;

public class GetSpecificationByIdQueryHandler
    : IRequestHandler<GetSpecificationByIdQuery, SpecificationDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IProjectAuthorizationService _auth;

    public GetSpecificationByIdQueryHandler(
        IApplicationDbContext context,
        IProjectAuthorizationService auth)
    {
        _context = context;
        _auth = auth;
    }

    public async Task<SpecificationDto?> Handle(
        GetSpecificationByIdQuery request,
        CancellationToken ct)
    {
        await _auth.RequireProjectMemberAsync(request.ProjectId, ct);

        var spec = await _context.ProjectSpecifications
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.Id == request.SpecificationId &&
                s.ProjectId == request.ProjectId, ct); // ProjectId в условии — защита от подмены

        if (spec is null) return null;

        return new SpecificationDto(
            spec.Id,
            spec.Version,
            spec.Content,
            spec.IsApproved,
            spec.CreatedAt,
            spec.IsApproved ? spec.ApprovedAt : null);
    }
}