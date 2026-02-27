using Application.Features.Users.Commands;
using Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Контроллер для управления пользователями.
/// </summary>
/// <remarks>
/// Все эндпоинты требуют аутентификации. Эндпоинты для просмотра
/// и удаления произвольных пользователей доступны только роли <c>Admin</c>.
/// </remarks>
[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="UsersController"/>.
    /// </summary>
    /// <param name="mediator">Экземпляр медиатора для отправки команд и запросов.</param>
    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Возвращает данные текущего аутентифицированного пользователя.
    /// </summary>
    /// <returns>
    /// <see cref="OkObjectResult"/> с данными текущего пользователя.
    /// </returns>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await _mediator.Send(new GetCurrentUserQuery());
        return Ok(result);
    }

    /// <summary>
    /// Обновляет данные текущего аутентифицированного пользователя.
    /// </summary>
    /// <param name="command">Команда обновления, содержащая новые данные пользователя.</param>
    /// <returns>
    /// <see cref="NoContentResult"/> в случае успешного обновления.
    /// </returns>
    [HttpPut("me")]
    public async Task<IActionResult> Update(UpdateUserCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Возвращает список всех пользователей системы.
    /// </summary>
    /// <returns>
    /// <see cref="OkObjectResult"/> с коллекцией всех пользователей.
    /// </returns>
    /// <remarks>
    /// Доступно только пользователям с ролью <c>Admin</c>.
    /// </remarks>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _mediator.Send(new GetAllUsersQuery()));
    }

    /// <summary>
    /// Возвращает пользователя по его идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор пользователя.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> с данными найденного пользователя.
    /// </returns>
    /// <remarks>
    /// Доступно только пользователям с ролью <c>Admin</c>.
    /// </remarks>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await _mediator.Send(new GetUserByIdQuery(id)));
    }

    /// <summary>
    /// Удаляет пользователя по его идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор удаляемого пользователя.</param>
    /// <returns>
    /// <see cref="NoContentResult"/> в случае успешного удаления.
    /// </returns>
    /// <remarks>
    /// Доступно только пользователям с ролью <c>Admin</c>.
    /// </remarks>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteUserCommand(id));
        return NoContent();
    }
}