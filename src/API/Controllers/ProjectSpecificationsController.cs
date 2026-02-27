using Application.Features.Projects.Specifications.Commands;
using Application.Features.Projects.Specifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Контроллер для управления спецификациями проекта.
/// </summary>
/// <remarks>
/// Все эндпоинты требуют аутентификации и привязаны к конкретному проекту
/// через маршрутный параметр <c>projectId</c>.
/// </remarks>
[Authorize]
[ApiController]
[Route("api/projects/{projectId:guid}/specifications")]
public class ProjectSpecificationsController : ControllerBase
{
    private readonly ISender _mediator;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ProjectSpecificationsController"/>.
    /// </summary>
    /// <param name="mediator">Экземпляр отправителя команд и запросов.</param>
    public ProjectSpecificationsController(ISender mediator)
        => _mediator = mediator;

    /// <summary>
    /// Возвращает список всех спецификаций указанного проекта.
    /// </summary>
    /// <param name="projectId">Уникальный идентификатор проекта.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// Коллекция <see cref="SpecificationDto"/> со спецификациями проекта,
    /// либо <see cref="NotFoundResult"/> если проект не найден.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SpecificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<SpecificationDto>>> GetSpecifications(
        Guid projectId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetProjectSpecificationsQuery(projectId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Возвращает спецификацию проекта по её идентификатору.
    /// </summary>
    /// <param name="projectId">Уникальный идентификатор проекта.</param>
    /// <param name="specificationId">Уникальный идентификатор спецификации.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="SpecificationDto"/> с данными спецификации,
    /// либо <see cref="NotFoundResult"/> если проект или спецификация не найдены.
    /// </returns>
    [HttpGet("{specificationId:guid}")]
    [ProducesResponseType(typeof(SpecificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpecificationDto>> GetSpecification(
        Guid projectId,
        Guid specificationId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetSpecificationByIdQuery(projectId, specificationId), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Создаёт новую спецификацию для указанного проекта.
    /// </summary>
    /// <param name="projectId">Уникальный идентификатор проекта.</param>
    /// <param name="command">Команда создания спецификации с необходимыми данными.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="CreatedAtActionResult"/> с данными созданной спецификации и ссылкой на неё.
    /// </returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateSpecificationResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateSpecificationResult>> CreateSpecification(
        Guid projectId,
        [FromBody] CreateSpecificationCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            command with { ProjectId = projectId }, ct);

        return CreatedAtAction(
            nameof(GetSpecification),
            new { projectId, specificationId = result.Id },
            result);
    }

    /// <summary>
    /// Утверждает спецификацию проекта.
    /// </summary>
    /// <param name="projectId">Уникальный идентификатор проекта.</param>
    /// <param name="specificationId">Уникальный идентификатор утверждаемой спецификации.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="ApproveSpecificationResult"/> с результатом утверждения,
    /// либо <see cref="BadRequestObjectResult"/> если утверждение невозможно.
    /// </returns>
    [HttpPost("{specificationId:guid}/approve")]
    [ProducesResponseType(typeof(ApproveSpecificationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApproveSpecificationResult>> ApproveSpecification(
        Guid projectId,
        Guid specificationId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ApproveSpecificationCommand(projectId, specificationId), ct);
        return Ok(result);
    }
}