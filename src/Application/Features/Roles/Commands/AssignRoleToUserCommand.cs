using Application.Common.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Roles.Commands;

/// <summary>
/// Команда для назначения роли указанному пользователю.
/// </summary>
/// <param name="UserId">Уникальный идентификатор пользователя.</param>
/// <param name="RoleId">Уникальный идентификатор назначаемой роли.</param>
public record AssignRoleToUserCommand(
    Guid UserId,
    Guid RoleId) : IRequest;

/// <summary>
/// Обработчик команды <see cref="AssignRoleToUserCommand"/>.
/// </summary>
public class AssignRoleToUserCommandHandler
    : IRequestHandler<AssignRoleToUserCommand>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="AssignRoleToUserCommandHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    public AssignRoleToUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Обрабатывает команду назначения роли пользователю.
    /// </summary>
    /// <param name="request">Команда с идентификаторами пользователя и роли.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <remarks>
    /// Операция идемпотентна — повторное назначение уже существующей связи
    /// пользователь-роль игнорируется без выброса исключения.
    /// </remarks>
    public async Task Handle(
        AssignRoleToUserCommand request,
        CancellationToken ct)
    {
        var exists = await _context.UserRoles
            .AnyAsync(x => x.UserId == request.UserId &&
                           x.RoleId == request.RoleId, ct);

        if (exists)
            return;

        var userRole = new UserRole
        {
            UserId = request.UserId,
            RoleId = request.RoleId
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync(ct);
    }
}