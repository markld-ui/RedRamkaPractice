namespace Application.Features.Projects.Commands.Shared;

/// <summary>
/// Результат выполнения перехода между стадиями проекта, возвращаемый из команд API.
/// </summary>
/// <param name="IsSuccess">
/// <see langword="true"/> если переход выполнен успешно; иначе <see langword="false"/>.
/// </param>
/// <param name="Error">
/// Сообщение об ошибке в случае неудачного перехода.
/// Равно <see langword="null"/> при успешном выполнении.
/// </param>
/// <param name="NewStage">
/// Название новой стадии проекта после успешного перехода.
/// Равно <see langword="null"/> в случае неудачи.
/// </param>
public record TransitionResult(
    bool IsSuccess,
    string? Error,
    string? NewStage);