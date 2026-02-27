using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Register;
using Application.Features.Auth.Commands.RefreshToken;
using MediatR;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Контроллер для управления аутентификацией и авторизацией пользователей.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="AuthController"/>.
    /// </summary>
    /// <param name="mediator">Экземпляр медиатора для отправки команд.</param>
    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Регистрирует нового пользователя в системе.
    /// </summary>
    /// <param name="command">Команда регистрации, содержащая данные нового пользователя.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> с результатом выполнения команды регистрации.
    /// </returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterCommand command,
        CancellationToken ct)
        => Ok(await _mediator.Send(command, ct));

    /// <summary>
    /// Выполняет вход пользователя в систему.
    /// </summary>
    /// <param name="command">Команда входа, содержащая учётные данные пользователя.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> с результатом выполнения команды входа,
    /// включая токены доступа и обновления.
    /// </returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginCommand command,
        CancellationToken ct)
        => Ok(await _mediator.Send(command, ct));

    /// <summary>
    /// Обновляет токен доступа по токену обновления.
    /// </summary>
    /// <param name="command">Команда обновления токена, содержащая действующий refresh-токен.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> с новой парой токенов доступа и обновления.
    /// </returns>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        RefreshTokenCommand command,
        CancellationToken ct)
        => Ok(await _mediator.Send(command, ct));
}