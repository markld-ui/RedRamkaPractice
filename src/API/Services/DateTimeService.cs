using Application.Common.Interfaces;

namespace API.Services;

/// <summary>
/// Сервис для получения текущего значения даты и времени.
/// </summary>
public class DateTimeService : IDateTime
{
    /// <summary>
    /// Возвращает текущие дату и время в формате UTC.
    /// </summary>
    /// <value>
    /// Значение <see cref="DateTime"/> в формате UTC на момент обращения к свойству.
    /// </value>
    public DateTime Now => DateTime.UtcNow;
}