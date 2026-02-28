using Stateless;

namespace Domain.Projects;

/// <summary>
/// Конечный автомат, управляющий переходами между стадиями жизненного цикла проекта.
/// </summary>
/// <remarks>
/// Построен на основе библиотеки <c>Stateless</c>. Допустимые переходы:
/// <list type="bullet">
///   <item><c>Design</c> → <c>Development</c> — <see cref="ProjectTrigger.StartDevelopment"/>.</item>
///   <item><c>Development</c> → <c>QA</c> — <see cref="ProjectTrigger.SendToQA"/>.</item>
///   <item><c>QA</c> → <c>Delivery</c> — <see cref="ProjectTrigger.PassQA"/>.</item>
///   <item><c>QA</c> → <c>Development</c> — <see cref="ProjectTrigger.FailQA"/> (с причиной).</item>
///   <item><c>Delivery</c> → <c>Support</c> — <see cref="ProjectTrigger.Release"/>.</item>
///   <item><c>Support</c> → <c>Design</c> — <see cref="ProjectTrigger.ReturnToDesign"/> (с причиной).</item>
///   <item><c>Support</c> → <c>Archived</c> — <see cref="ProjectTrigger.Archive"/> (с причиной).</item>
/// </list>
/// </remarks>
public class ProjectStateMachine
{
    private readonly StateMachine<ProjectStage, ProjectTrigger> _machine;

    private readonly StateMachine<ProjectStage, ProjectTrigger>
        .TriggerWithParameters<string> _failQa;

    private readonly StateMachine<ProjectStage, ProjectTrigger>
        .TriggerWithParameters<string> _returnToDesign;

    private readonly StateMachine<ProjectStage, ProjectTrigger>
        .TriggerWithParameters<string> _archive;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ProjectStateMachine"/>.
    /// </summary>
    /// <param name="stateAccessor">Функция, возвращающая текущую стадию проекта.</param>
    /// <param name="stateMutator">Действие, устанавливающее новую стадию проекта.</param>
    public ProjectStateMachine(
        Func<ProjectStage> stateAccessor,
        Action<ProjectStage> stateMutator)
    {
        _machine = new StateMachine<ProjectStage, ProjectTrigger>(
            stateAccessor,
            stateMutator);

        _failQa = _machine.SetTriggerParameters<string>(ProjectTrigger.FailQA);
        _returnToDesign = _machine.SetTriggerParameters<string>(ProjectTrigger.ReturnToDesign);
        _archive = _machine.SetTriggerParameters<string>(ProjectTrigger.Archive);

        Configure();
    }

    /// <summary>
    /// Настраивает допустимые переходы между стадиями.
    /// </summary>
    private void Configure()
    {
        _machine.Configure(ProjectStage.Design)
            .Permit(ProjectTrigger.StartDevelopment, ProjectStage.Development);

        _machine.Configure(ProjectStage.Development)
            .Permit(ProjectTrigger.SendToQA, ProjectStage.QA);

        _machine.Configure(ProjectStage.QA)
            .Permit(ProjectTrigger.PassQA, ProjectStage.Delivery)
            .Permit(ProjectTrigger.FailQA, ProjectStage.Development);

        _machine.Configure(ProjectStage.Delivery)
            .Permit(ProjectTrigger.Release, ProjectStage.Support);

        _machine.Configure(ProjectStage.Support)
            .Permit(ProjectTrigger.ReturnToDesign, ProjectStage.Design)
            .Permit(ProjectTrigger.Archive, ProjectStage.Archived);

        _machine.Configure(ProjectStage.Archived);
    }

    /// <summary>
    /// Проверяет, допустим ли указанный триггер из текущей стадии.
    /// </summary>
    /// <param name="trigger">Проверяемый триггер.</param>
    /// <returns>
    /// <see langword="true"/> если переход по триггеру разрешён из текущей стадии;
    /// иначе <see langword="false"/>.
    /// </returns>
    public bool CanFire(ProjectTrigger trigger)
        => _machine.CanFire(trigger);

    /// <summary>
    /// Выполняет переход по указанному триггеру без дополнительных параметров.
    /// </summary>
    /// <param name="trigger">Триггер, инициирующий переход.</param>
    public void Fire(ProjectTrigger trigger)
        => _machine.Fire(trigger);

    /// <summary>
    /// Выполняет переход по триггеру <see cref="ProjectTrigger.FailQA"/> с указанием причины.
    /// </summary>
    /// <param name="reason">Причина провала тестирования.</param>
    public void FireFailQA(string reason)
        => _machine.Fire(_failQa, reason);

    /// <summary>
    /// Выполняет переход по триггеру <see cref="ProjectTrigger.ReturnToDesign"/> с указанием причины.
    /// </summary>
    /// <param name="reason">Причина возврата на стадию проектирования.</param>
    public void FireReturnToDesign(string reason)
        => _machine.Fire(_returnToDesign, reason);

    /// <summary>
    /// Выполняет переход по триггеру <see cref="ProjectTrigger.Archive"/> с указанием причины.
    /// </summary>
    /// <param name="reason">Причина архивирования проекта.</param>
    public void FireArchive(string reason)
        => _machine.Fire(_archive, reason);
}