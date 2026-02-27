using Application.Features.Roles.Commands;
using Application.Features.Roles.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Контроллер для управления ролями пользователей.
/// </summary>
/// <remarks>
/// Все эндпоинты доступны только пользователям с ролью <c>Admin</c>.
/// </remarks>
[ApiController]
[Route("api/roles")]
[Authorize(Roles = "Admin")]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="RolesController"/>.
    /// </summary>
    /// <param name="mediator">Экземпляр медиатора для отправки команд и запросов.</param>
    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Создаёт новую роль.
    /// </summary>
    /// <param name="command">Команда создания роли с необходимыми данными.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> с идентификатором созданной роли.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> Create(CreateRoleCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(id);
    }

    /// <summary>
    /// Назначает роль указанному пользователю.
    /// </summary>
    /// <param name="command">Команда, содержащая идентификаторы пользователя и назначаемой роли.</param>
    /// <returns>
    /// <see cref="NoContentResult"/> в случае успешного назначения роли.
    /// </returns>
    [HttpPost("assign")]
    public async Task<IActionResult> Assign(AssignRoleToUserCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Возвращает список всех существующих ролей.
    /// </summary>
    /// <returns>
    /// <see cref="OkObjectResult"/> с коллекцией всех ролей.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _mediator.Send(new GetAllRolesQuery()));
    }

    /// <summary>
    /// Удаляет роль по её идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор удаляемой роли.</param>
    /// <returns>
    /// <see cref="NoContentResult"/> в случае успешного удаления роли.
    /// </returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteRoleCommand(id));
        return NoContent();
    }
}