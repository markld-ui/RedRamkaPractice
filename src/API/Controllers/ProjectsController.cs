using Application.Features.Projects.Commands;
using Application.Features.Projects.Commands.Shared;
using Application.Features.Projects.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize] // ← все эндпоинты требуют аутентификации на уровне ASP.NET
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly ISender _mediator;

    public ProjectsController(ISender mediator)
        => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GetProjectDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GetProjectDto>>> GetProjects(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProjectsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetProjectDto>> GetProject(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProjectByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

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

    [HttpPost("{id:guid}/stage/start-development")]
    [ProducesResponseType(typeof(TransitionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransitionResult>> StartDevelopment(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new StartDevelopmentCommand(id), ct);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/stage/send-to-qa")]
    [ProducesResponseType(typeof(TransitionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransitionResult>> SendToQA(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new SendToQACommand(id), ct);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/stage/pass-qa")]
    [ProducesResponseType(typeof(TransitionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransitionResult>> PassQA(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new PassQACommand(id), ct);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

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

    [HttpPost("{id:guid}/stage/release")]
    [ProducesResponseType(typeof(TransitionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransitionResult>> Release(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ReleaseCommand(id), ct);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

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

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId, CancellationToken ct)
    {
        await _mediator.Send(new RemoveProjectMemberCommand(id, userId), ct);
        return NoContent();
    }
}