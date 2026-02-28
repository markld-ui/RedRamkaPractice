namespace Domain.Projects;

/// <summary>
/// Стадия жизненного цикла проекта.
/// </summary>
public enum ProjectStage
{
    /// <summary>Проектирование — начальная стадия разработки требований и архитектуры.</summary>
    Design = 1,

    /// <summary>Разработка — стадия реализации функциональности.</summary>
    Development = 2,

    /// <summary>Тестирование — стадия проверки качества (QA).</summary>
    QA = 3,

    /// <summary>Поставка — стадия передачи продукта заказчику.</summary>
    Delivery = 4,

    /// <summary>Сопровождение — стадия поддержки и обслуживания продукта.</summary>
    Support = 5,

    /// <summary>Архив — завершённый или приостановленный проект.</summary>
    Archived = 6
}