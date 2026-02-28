namespace Domain.Projects;

/// <summary>
/// Результат выполнения перехода между стадиями проекта.
/// </summary>
public class ProjectTransitionResult
{
    /// <summary>
    /// Признак успешности перехода.
    /// <see langword="true"/> если переход выполнен успешно; иначе <see langword="false"/>.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Сообщение об ошибке в случае неудачного перехода.
    /// Равно <see langword="null"/> при успешном выполнении.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Новая стадия проекта после успешного перехода.
    /// Равна <see langword="null"/> в случае неудачи.
    /// </summary>
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

    /// <summary>
    /// Создаёт успешный результат перехода с указанием новой стадии.
    /// </summary>
    /// <param name="stage">Новая стадия проекта после перехода.</param>
    /// <returns>Экземпляр <see cref="ProjectTransitionResult"/> с признаком успеха.</returns>
    public static ProjectTransitionResult Success(ProjectStage stage) =>
        new(true, null, stage);

    /// <summary>
    /// Создаёт неудачный результат перехода с описанием причины.
    /// </summary>
    /// <param name="error">Сообщение об ошибке, описывающее причину невозможности перехода.</param>
    /// <returns>Экземпляр <see cref="ProjectTransitionResult"/> с признаком неудачи.</returns>
    public static ProjectTransitionResult Fail(string error) =>
        new(false, error, null);
}