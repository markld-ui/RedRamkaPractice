using Domain.Models;

namespace Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserEmail { get; }
    bool IsAuthenticated { get; }
    Task<bool> IsInRoleAsync(string role);
}