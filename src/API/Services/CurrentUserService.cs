using System.Security.Claims;
using Application.Common.Interfaces;

namespace API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor  httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

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
    
    public string? UserEmail => _httpContextAccessor.HttpContext?.User?
        .FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated => UserId.HasValue;

    public Task<bool> IsInRoleAsync(string role)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return Task.FromResult(false);

        if (user.IsInRole(role)) return Task.FromResult(true);
        return Task.FromResult(false);
    }
}