namespace Domain.Projects;

public class ProjectTransitionResult
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public ProjectStage? NewStage { get; }
    private ProjectTransitionResult(
        bool isSuccess,
        string? error,
        ProjectStage? newStage)
    {
        IsSuccess = isSuccess;
        Error = error;
        NewStage = newStage;
    }
    public static ProjectTransitionResult Success(ProjectStage stage) =>
        new(true, null, stage);
    public static ProjectTransitionResult Fail(string error) =>
        new(false, error, null);
}
