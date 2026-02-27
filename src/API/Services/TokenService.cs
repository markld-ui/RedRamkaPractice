using Application.Common.Interfaces;
using Domain.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services;

/// <summary>
/// Сервис для генерации JWT-токенов доступа.
/// </summary>
/// <remarks>
/// Параметры токена (ключ подписи, издатель, аудитория) считываются
/// из конфигурации приложения в секции <c>Jwt</c>.
/// </remarks>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TokenService"/>.
    /// </summary>
    /// <param name="configuration">Конфигурация приложения.</param>
    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Генерирует JWT-токен доступа для указанного пользователя.
    /// </summary>
    /// <param name="user">Пользователь, для которого выпускается токен.</param>
    /// <returns>
    /// Строка с подписанным JWT-токеном, содержащим клеймы идентификатора,
    /// электронной почты, имени пользователя и его ролей. Срок действия токена — 2 часа.
    /// </returns>
    /// <remarks>
    /// Токен подписывается алгоритмом <see cref="SecurityAlgorithms.HmacSha256"/>
    /// с использованием симметричного ключа из конфигурации (<c>Jwt:Key</c>).
    /// В токен включаются следующие клеймы:
    /// <list type="bullet">
    ///   <item><see cref="ClaimTypes.NameIdentifier"/> — идентификатор пользователя.</item>
    ///   <item><see cref="ClaimTypes.Email"/> — адрес электронной почты.</item>
    ///   <item><see cref="ClaimTypes.Name"/> — полное имя пользователя.</item>
    ///   <item><see cref="ClaimTypes.Role"/> — роль пользователя (по одному клейму на каждую роль).</item>
    /// </list>
    /// </remarks>
    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var creds = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Credentials.Email),
            new Claim(ClaimTypes.Name,
                $"{user.FirstName} {user.LastName}")
        };

        foreach (var userRole in user.UserRoles)
        {
            claims.Add(new Claim(
                ClaimTypes.Role,
                userRole.Role.Name));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}