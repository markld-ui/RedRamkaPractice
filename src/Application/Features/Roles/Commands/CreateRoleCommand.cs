using Application.Common.Interfaces;
using Domain.Models;
using MediatR;

namespace Application.Features.Roles.Commands;

/// <summary>
/// Команда для создания новой роли в системе.
/// </summary>
/// <param name="Name">Название создаваемой роли.</param>
/// <param name="Description">Описание роли и её зоны ответственности.</param>
public record CreateRoleCommand(
    string Name,
    string Description
) : IRequest<Guid>;

/// <summary>
/// Обработчик команды <see cref="CreateRoleCommand"/>.
/// </summary>
public class CreateRoleCommandHandler
    : IRequestHandler<CreateRoleCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CreateRoleCommandHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    public CreateRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Обрабатывает команду создания новой роли.
    /// </summary>
    /// <param name="request">Команда с названием и описанием создаваемой роли.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// Уникальный идентификатор созданной роли.
    /// </returns>
    public async Task<Guid> Handle(
        CreateRoleCommand request,
        CancellationToken ct)
    {
        var role = new Role
        {
            Name = request.Name,
            Description = request.Description
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(ct);

        return role.Id;
    }
}