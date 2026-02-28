namespace Application.Common.Interfaces;

/// <summary>
/// Интерфейс сервиса для получения текущего значения даты и времени.
/// </summary>
/// <remarks>
/// Абстрагирует обращение к системному времени, что позволяет
/// подменять реализацию в тестах для воспроизводимых результатов.
/// </remarks>
public interface IDateTime
{
    /// <summary>
    /// Возвращает текущие дату и время.
    /// </summary>
    DateTime Now { get; }
}