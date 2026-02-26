using MediatR;

namespace Application.Features.Projects.Queries;

public record GetProjectsQuery : IRequest<IEnumerable<GetProjectDto>>;
