using System.Security.Claims;
using Application.Common.Interfaces;

namespace API.Services;

/// <summary>
/// Сервис для получения данных о текущем аутентифицированном пользователе
/// из контекста HTTP-запроса.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CurrentUserService"/>.
    /// </summary>
    /// <param name="httpContextAccessor">Средство доступа к текущему HTTP-контексту.</param>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Возвращает уникальный идентификатор текущего пользователя,
    /// извлечённый из клейма <see cref="ClaimTypes.NameIdentifier"/>.
    /// </summary>
    /// <value>
    /// <see cref="Guid"/> пользователя, либо <see langword="null"/>
    /// если пользователь не аутентифицирован или клейм отсутствует.
    /// </value>
    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(userIdClaim, out var id)) return id;

            return null;
        }
    }

    /// <summary>
    /// Возвращает адрес электронной почты текущего пользователя,
    /// извлечённый из клейма <see cref="ClaimTypes.Email"/>.
    /// </summary>
    /// <value>
    /// Строка с адресом электронной почты, либо <see langword="null"/>
    /// если пользователь не аутентифицирован или клейм отсутствует.
    /// </value>
    public string? UserEmail => _httpContextAccessor.HttpContext?.User?
        .FindFirstValue(ClaimTypes.Email);

    /// <summary>
    /// Возвращает значение, указывающее, аутентифицирован ли текущий пользователь.
    /// </summary>
    /// <value>
    /// <see langword="true"/> если идентификатор пользователя успешно получен;
    /// иначе <see langword="false"/>.
    /// </value>
    public bool IsAuthenticated => UserId.HasValue;

    /// <summary>
    /// Проверяет, принадлежит ли текущий пользователь к указанной роли.
    /// </summary>
    /// <param name="role">Название проверяемой роли.</param>
    /// <returns>
    /// <see langword="true"/> если пользователь аутентифицирован и состоит в указанной роли;
    /// иначе <see langword="false"/>.
    /// </returns>
    public Task<bool> IsInRoleAsync(string role)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return Task.FromResult(false);

        if (user.IsInRole(role)) return Task.FromResult(true);
        return Task.FromResult(false);
    }
}