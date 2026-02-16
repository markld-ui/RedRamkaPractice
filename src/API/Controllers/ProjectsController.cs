using Application.Features.Projects.Commands.CreateProject;
using Application.Features.Projects.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly ISender _mediator;

    public ProjectsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateProjectResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateProjectResult>> CreateProject(
        [FromBody] CreateProjectCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        return CreatedAtAction(
            nameof(GetProject),
            new { id = result.Id },
            result);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetProjectDto>> GetProject(Guid id)
    {
        var result = await _mediator.Send(new GetProjectByIdQuery(id));
        if (result == null) return NotFound();

        return Ok(result);
    }
}