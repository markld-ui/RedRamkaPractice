namespace Domain.Projects;

/// <summary>
/// Триггер, инициирующий переход между стадиями жизненного цикла проекта.
/// </summary>
public enum ProjectTrigger
{
    /// <summary>Запуск разработки. Переход: <c>Design</c> → <c>Development</c>.</summary>
    StartDevelopment,

    /// <summary>Передача на тестирование. Переход: <c>Development</c> → <c>QA</c>.</summary>
    SendToQA,

    /// <summary>Провал тестирования. Переход: <c>QA</c> → <c>Development</c>. Требует указания причины.</summary>
    FailQA,

    /// <summary>Успешное прохождение тестирования. Переход: <c>QA</c> → <c>Delivery</c>.</summary>
    PassQA,

    /// <summary>Выпуск релиза. Переход: <c>Delivery</c> → <c>Support</c>.</summary>
    Release,

    /// <summary>Возврат на проектирование. Переход: <c>Support</c> → <c>Design</c>. Требует указания причины.</summary>
    ReturnToDesign,

    /// <summary>Архивирование проекта. Переход: <c>Support</c> → <c>Archived</c>. Требует указания причины.</summary>
    Archive
}