using Domain.Projects;
using MediatR;

namespace Application.Features.Projects.Commands.CreateProject;

public record CreateProjectCommand(
    string Name,
    string Description,
    List<Guid> MemberIds) : IRequest<CreateProjectResult>;

public record CreateProjectResult(
    Guid Id,
    string Name,
    ProjectStage Stage,
    DateTime CreatedAt);