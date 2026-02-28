using Domain.Common;
using Domain.Events;

namespace Domain.Projects;

/// <summary>
/// Доменная сущность проекта, управляющая его жизненным циклом.
/// </summary>
/// <remarks>
/// Проект проходит через стадии посредством конечного автомата <see cref="ProjectStateMachine"/>.
/// Каждый переход между стадиями фиксируется в коллекции <see cref="Transitions"/>.
/// При создании проекта автоматически публикуется событие <see cref="ProjectCreatedEvent"/>.
/// </remarks>
public class Project : BaseEntity
{
    private readonly List<ProjectTransition> _transitions = new();
    private readonly List<ProjectSpecification> _specifications = new();
    private readonly List<ProjectMember> _members = new();

    private Project() { } // EF

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="Project"/>.
    /// Начальная стадия проекта — <see cref="ProjectStage.Design"/>.
    /// </summary>
    /// <param name="name">Название проекта.</param>
    /// <param name="description">Описание проекта.</param>
    public Project(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Stage = ProjectStage.Design;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProjectCreatedEvent(this));
    }

    /// <summary>Уникальный идентификатор проекта.</summary>
    public Guid Id { get; private set; }

    /// <summary>Название проекта.</summary>
    public string Name { get; private set; } = null!;

    /// <summary>Описание проекта.</summary>
    public string Description { get; private set; } = null!;

    /// <summary>Текущая стадия жизненного цикла проекта.</summary>
    public ProjectStage Stage { get; private set; }

    /// <summary>Дата и время создания проекта в формате UTC.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Дата и время последнего изменения проекта в формате UTC.</summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Дата и время архивирования проекта в формате UTC.
    /// Равно <see langword="null"/>, если проект не архивирован.
    /// </summary>
    public DateTime? ArchivedAt { get; private set; }

    /// <summary>История переходов между стадиями проекта.</summary>
    public IReadOnlyCollection<ProjectTransition> Transitions => _transitions;

    /// <summary>Коллекция спецификаций проекта.</summary>
    public IReadOnlyCollection<ProjectSpecification> Specifications => _specifications;

    /// <summary>Коллекция участников проекта.</summary>
    public IReadOnlyCollection<ProjectMember> Members => _members;

    /// <summary>
    /// Создаёт экземпляр конечного автомата, привязанного к текущей стадии проекта.
    /// </summary>
    private ProjectStateMachine CreateStateMachine()
        => new(() => Stage, s => Stage = s);

    /// <summary>
    /// Регистрирует переход между стадиями и обновляет <see cref="UpdatedAt"/>.
    /// </summary>
    /// <param name="from">Исходная стадия.</param>
    /// <param name="to">Целевая стадия.</param>
    /// <param name="reason">Причина перехода (опционально).</param>
    private void AddTransition(ProjectStage from, ProjectStage to, string? reason)
    {
        _transitions.Add(new ProjectTransition(Id, from, to, reason));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Добавляет новую спецификацию к проекту с автоматически вычисленным номером версии.
    /// </summary>
    /// <param name="content">Содержимое спецификации.</param>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если проект находится в стадии <see cref="ProjectStage.Archived"/>.
    /// </exception>
    public void AddSpecification(string content)
    {
        if (Stage == ProjectStage.Archived)
            throw new InvalidOperationException("Cannot add specification to an archived project.");

        var nextVersion = _specifications.Any()
            ? _specifications.Max(s => s.Version) + 1
            : 1;

        _specifications.Add(new ProjectSpecification(nextVersion, content, Id));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Утверждает спецификацию по её идентификатору.
    /// Ранее утверждённая спецификация автоматически отзывается,
    /// так как актуальной может быть только одна.
    /// </summary>
    /// <param name="specificationId">Идентификатор утверждаемой спецификации.</param>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если спецификация не найдена или уже утверждена.
    /// </exception>
    public void ApproveSpecification(Guid specificationId)
    {
        var spec = _specifications.FirstOrDefault(s => s.Id == specificationId);

        if (spec is null)
            throw new InvalidOperationException("Specification not found in this project.");

        if (spec.IsApproved)
            throw new InvalidOperationException("Specification is already approved.");

        foreach (var approved in _specifications.Where(s => s.IsApproved))
            approved.Revoke();

        spec.Approve();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Проверяет наличие хотя бы одной утверждённой спецификации в проекте.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> если в проекте есть утверждённая спецификация;
    /// иначе <see langword="false"/>.
    /// </returns>
    public bool HasApprovedSpecification()
        => _specifications.Any(s => s.IsApproved);

    /// <summary>
    /// Добавляет участника в проект с указанной ролью.
    /// </summary>
    /// <param name="userId">Идентификатор добавляемого пользователя.</param>
    /// <param name="roleId">Идентификатор роли участника в проекте.</param>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если пользователь уже является участником проекта.
    /// </exception>
    public void AddMember(Guid userId, Guid roleId)
    {
        if (_members.Any(m => m.UserId == userId))
            throw new InvalidOperationException("User already assigned to project.");

        _members.Add(new ProjectMember(Id, userId, roleId));
    }

    /// <summary>
    /// Переводит проект в стадию разработки.
    /// Переход возможен только при наличии утверждённой спецификации.
    /// </summary>
    /// <returns>Результат выполнения перехода.</returns>
    public ProjectTransitionResult StartDevelopment()
    {
        if (!HasApprovedSpecification())
            return ProjectTransitionResult.Fail("No approved specification.");

        return Execute(ProjectTrigger.StartDevelopment);
    }

    /// <summary>Переводит проект на стадию тестирования (QA).</summary>
    /// <returns>Результат выполнения перехода.</returns>
    public ProjectTransitionResult SendToQA()
        => Execute(ProjectTrigger.SendToQA);

    /// <summary>Фиксирует успешное прохождение тестирования (QA).</summary>
    /// <returns>Результат выполнения перехода.</returns>
    public ProjectTransitionResult PassQA()
        => Execute(ProjectTrigger.PassQA);

    /// <summary>Переводит проект в стадию релиза.</summary>
    /// <returns>Результат выполнения перехода.</returns>
    public ProjectTransitionResult Release()
        => Execute(ProjectTrigger.Release);

    /// <summary>Фиксирует провал тестирования (QA) с указанием причины.</summary>
    /// <param name="reason">Причина провала тестирования.</param>
    /// <returns>Результат выполнения перехода.</returns>
    public ProjectTransitionResult FailQA(string reason)
        => Execute(ProjectTrigger.FailQA, reason);

    /// <summary>Возвращает проект на стадию проектирования с указанием причины.</summary>
    /// <param name="reason">Причина возврата на проектирование.</param>
    /// <returns>Результат выполнения перехода.</returns>
    public ProjectTransitionResult ReturnToDesign(string reason)
        => Execute(ProjectTrigger.ReturnToDesign, reason);

    /// <summary>
    /// Архивирует проект с указанием причины.
    /// При успешном переходе фиксирует время архивирования в <see cref="ArchivedAt"/>.
    /// </summary>
    /// <param name="reason">Причина архивирования проекта.</param>
    /// <returns>Результат выполнения перехода.</returns>
    public ProjectTransitionResult Archive(string reason)
    {
        var result = Execute(ProjectTrigger.Archive, reason);

        if (result.IsSuccess)
            ArchivedAt = DateTime.UtcNow;

        return result;
    }

    /// <summary>
    /// Выполняет переход между стадиями через конечный автомат
    /// и регистрирует его в истории переходов.
    /// </summary>
    /// <param name="trigger">Триггер, инициирующий переход.</param>
    /// <param name="reason">Причина перехода (обязательна для триггеров
    /// <see cref="ProjectTrigger.FailQA"/>, <see cref="ProjectTrigger.ReturnToDesign"/>
    /// и <see cref="ProjectTrigger.Archive"/>).</param>
    /// <returns>
    /// <see cref="ProjectTransitionResult.Success"/> с новой стадией при успешном переходе,
    /// либо <see cref="ProjectTransitionResult.Fail"/> если переход невозможен из текущей стадии.
    /// </returns>
    private ProjectTransitionResult Execute(ProjectTrigger trigger, string? reason = null)
    {
        var machine = CreateStateMachine();
        var from = Stage;

        if (!machine.CanFire(trigger))
            return ProjectTransitionResult.Fail(
                $"Cannot execute {trigger} from {Stage}");

        switch (trigger)
        {
            case ProjectTrigger.FailQA:
                machine.FireFailQA(reason!);
                break;

            case ProjectTrigger.ReturnToDesign:
                machine.FireReturnToDesign(reason!);
                break;

            case ProjectTrigger.Archive:
                machine.FireArchive(reason!);
                break;

            default:
                machine.Fire(trigger);
                break;
        }

        AddTransition(from, Stage, reason);

        return ProjectTransitionResult.Success(Stage);
    }
}