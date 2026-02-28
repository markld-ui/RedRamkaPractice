namespace Application.Common.Constants;

/// <summary>
/// Константы названий системных ролей.
/// </summary>
public static class RoleConstants
{
    /// <summary>Менеджер проекта — управляет жизненным циклом проекта и командой.</summary>
    public const string ProjectManager = "ProjectManager";

    /// <summary>Разработчик — реализует функциональность проекта.</summary>
    public const string Developer = "Developer";

    /// <summary>Тестировщик — выполняет проверку качества (QA).</summary>
    public const string Tester = "Tester";

    /// <summary>Продуктовый менеджер — формирует требования к продукту.</summary>
    public const string ProductManager = "ProductManager";

    /// <summary>DevOps-инженер — управляет инфраструктурой и процессами развёртывания.</summary>
    public const string DevOps = "DevOps";

    /// <summary>Администратор системы — обладает полным доступом ко всем функциям.</summary>
    public const string Admin = "Admin";
}