using Application.Features.Credentials.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Контроллер для управления учётными данными пользователей.
/// </summary>
/// <remarks>
/// Все эндпоинты требуют аутентификации.
/// </remarks>
[Authorize]
[ApiController]
[Route("api/credentials")]
public class CredentialsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CredentialsController"/>.
    /// </summary>
    /// <param name="mediator">Экземпляр медиатора для отправки команд.</param>
    public CredentialsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Изменяет пароль текущего аутентифицированного пользователя.
    /// </summary>
    /// <param name="command">Команда смены пароля, содержащая старый и новый пароли.</param>
    /// <returns>
    /// <see cref="NoContentResult"/> в случае успешного изменения пароля.
    /// </returns>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Отзывает refresh-токен указанного пользователя.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя, чей токен необходимо отозвать.</param>
    /// <returns>
    /// <see cref="NoContentResult"/> в случае успешного отзыва токена.
    /// </returns>
    /// <remarks>
    /// Доступно только пользователям с ролью <c>Admin</c>.
    /// </remarks>
    [HttpPost("revoke/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Revoke(Guid userId)
    {
        await _mediator.Send(
            new RevokeRefreshTokenCommand(userId));

        return NoContent();
    }
}