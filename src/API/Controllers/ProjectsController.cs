using Application.Features.Projects.Commands;
using Application.Features.Projects.Commands.Shared;
using Application.Features.Projects.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Контроллер для управления проектами и их жизненным циклом.
/// </summary>
/// <remarks>
/// Все эндпоинты требуют аутентификации.
/// </remarks>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly ISender _mediator;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ProjectsController"/>.
    /// </summary>
    /// <param name="mediator">Экземпляр отправителя команд и запросов.</param>
    public ProjectsController(ISender mediator)
        => _mediator = mediator;

    /// <summary>
    /// Возвращает список всех проектов.
    /// </summary>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// Коллекция <see cref="GetProjectDto"/> со всеми доступными проектами.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GetProjectDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GetProjectDto>>> GetProjects(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProjectsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Возвращает проект по его идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор проекта.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="GetProjectDto"/> с данными проекта,
    /// либо <see cref="NotFoundResult"/> если проект не найден.
    /// </returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetProjectDto>> GetProject(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProjectByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Создаёт новый проект.
    /// </summary>
    /// <param name="command">Команда создания проекта с необходимыми данными.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="CreatedAtActionResult"/> с данными созданного проекта и ссылкой на него.
    /// </returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateProjectResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateProjectResult>> CreateProject(
        [FromBody] CreateProjectCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetProject), new { id = result.Id }, result);
    }

    /// <summary>
    /// Переводит проект в стадию разработки.
    /// </summary>
    /// <param name="id">Уникальный идентификатор проекта.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="TransitionResult"/> с результатом перехода,
    /// либо <see cref="BadRequestObjectResult"/> если переход невозможен.
    /// </returns>
    [HttpPost("{id:guid}/stage/start-development")]
    [ProducesResponseType(typeof(TransitionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransitionResult>> StartDevelopment(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new StartDevelopmentCommand(id), ct);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Переводит проект на стадию тестирования (QA).
    /// </summary>
    /// <param name="id">Уникальный идентификатор проекта.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="TransitionResult"/> с результатом перехода,
    /// либо <see cref="BadRequestObjectResult"/> если переход невозможен.
    /// </returns>
    [HttpPost("{id:guid}/stage/send-to-qa")]
    [ProducesResponseType(typeof(TransitionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransitionResult>> SendToQA(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new SendToQACommand(id), ct);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Фиксирует успешное прохождение тестирования (QA).
    /// </summary>
    /// <param name="id">Уникальный идентификатор проекта.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="TransitionResult"/> с результатом перехода,
    /// либо <see cref="BadRequestObjectResult"/> если переход невозможен.
    /// </returns>
    [HttpPost("{id:guid}/stage/pass-qa")]
    [ProducesResponseType(typeof(TransitionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransitionResult>> PassQA(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new PassQACommand(id), ct);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Фиксирует провал тестирования (QA) с указанием причины.
    /// </summary>
    /// <param name="id">Уникальный идентификатор проекта.</param>
    /// <param name="command">Команда, содержащая причину провала тестирования.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="TransitionResult"/> с результатом перехода,
    /// либо <see cref="BadRequestObjectResult"/> если переход невозможен.
    /// </returns>
    [HttpPost("{id:guid}/stage/fail-qa")]
    [ProducesResponseType(typeof(TransitionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransitionResult>> FailQA(
        Guid id,
        [FromBody] FailQACommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command with { ProjectId = id }, ct);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Переводит проект в стадию релиза.
    /// </summary>
    /// <param name="id">Уникальный идентификатор проекта.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="TransitionResult"/> с результатом перехода,
    /// либо <see cref="BadRequestObjectResult"/> если переход невозможен.
    /// </returns>
    [HttpPost("{id:guid}/stage/release")]
    [ProducesResponseType(typeof(TransitionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransitionResult>> Release(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ReleaseCommand(id), ct);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Возвращает проект на стадию проектирования с указанием причины.
    /// </summary>
    /// <param name="id">Уникальный идентификатор проекта.</param>
    /// <param name="command">Команда, содержащая причину возврата на проектирование.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="TransitionResult"/> с результатом перехода,
    /// либо <see cref="BadRequestObjectResult"/> если переход невозможен.
    /// </returns>
    [HttpPost("{id:guid}/stage/return-to-design")]
    [ProducesResponseType(typeof(TransitionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransitionResult>> ReturnToDesign(
        Guid id,
        [FromBody] ReturnToDesignCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command with { ProjectId = id }, ct);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Архивирует проект с указанием причины.
    /// </summary>
    /// <param name="id">Уникальный идентификатор проекта.</param>
    /// <param name="command">Команда, содержащая причину архивирования.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="TransitionResult"/> с результатом перехода,
    /// либо <see cref="BadRequestObjectResult"/> если переход невозможен.
    /// </returns>
    [HttpPost("{id:guid}/stage/archive")]
    [ProducesResponseType(typeof(TransitionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransitionResult>> Archive(
        Guid id,
        [FromBody] ArchiveCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command with { ProjectId = id }, ct);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Добавляет участника в проект.
    /// </summary>
    /// <param name="id">Уникальный идентификатор проекта.</param>
    /// <param name="command">Команда, содержащая данные добавляемого участника.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="NoContentResult"/> в случае успешного добавления участника.
    /// </returns>
    [HttpPost("{id:guid}/members")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddMember(
        Guid id,
        [FromBody] AddProjectMemberCommand command,
        CancellationToken ct)
    {
        await _mediator.Send(command with { ProjectId = id }, ct);
        return NoContent();
    }

    /// <summary>
    /// Удаляет участника из проекта.
    /// </summary>
    /// <param name="id">Уникальный идентификатор проекта.</param>
    /// <param name="userId">Уникальный идентификатор удаляемого участника.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="NoContentResult"/> в случае успешного удаления участника.
    /// </returns>
    [HttpDelete("{id:guid}/members/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId, CancellationToken ct)
    {
        await _mediator.Send(new RemoveProjectMemberCommand(id, userId), ct);
        return NoContent();
    }
}