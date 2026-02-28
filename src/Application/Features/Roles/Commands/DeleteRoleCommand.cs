using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Roles.Commands;

/// <summary>
/// Команда для удаления роли по её идентификатору.
/// </summary>
/// <param name="Id">Уникальный идентификатор удаляемой роли.</param>
public record DeleteRoleCommand(Guid Id) : IRequest;

/// <summary>
/// Обработчик команды <see cref="DeleteRoleCommand"/>.
/// </summary>
public class DeleteRoleCommandHandler
    : IRequestHandler<DeleteRoleCommand>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="DeleteRoleCommandHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    public DeleteRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Обрабатывает команду удаления роли.
    /// </summary>
    /// <param name="request">Команда с идентификатором удаляемой роли.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если роль с указанным идентификатором не найдена.
    /// </exception>
    public async Task Handle(
        DeleteRoleCommand request,
        CancellationToken ct)
    {
        var role = await _context.Roles
            .FirstAsync(x => x.Id == request.Id, ct);

        _context.Roles.Remove(role);

        await _context.SaveChangesAsync(ct);
    }
}