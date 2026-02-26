using Application.Features.Credentials.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/credentials")]
public class CredentialsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CredentialsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("revoke/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Revoke(Guid userId)
    {
        await _mediator.Send(
            new RevokeRefreshTokenCommand(userId));

        return NoContent();
    }
}