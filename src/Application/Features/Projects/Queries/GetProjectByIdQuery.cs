using MediatR;

namespace Application.Features.Projects.Queries;

public record GetProjectByIdQuery(Guid Id) : IRequest<GetProjectDto?>;

public record GetProjectDto(Guid Id,
    string Name,
    string Description,
    string Stage,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IEnumerable<Guid> MemberIds);