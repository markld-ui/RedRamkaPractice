
namespace Application.Features.Projects.Commands.Shared;

public record TransitionResult(
    bool IsSuccess,
    string? Error,
    string? NewStage);
