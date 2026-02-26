using Application.Features.Projects.Specifications.Commands;
using Application.Features.Projects.Specifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId:guid}/specifications")]
public class ProjectSpecificationsController : ControllerBase
{
    private readonly ISender _mediator;

    public ProjectSpecificationsController(ISender mediator)
        => _mediator = mediator;

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