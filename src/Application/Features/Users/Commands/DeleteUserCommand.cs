using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Users.Commands;

/// <summary>
/// Команда для удаления пользователя по его идентификатору.
/// </summary>
/// <param name="Id">Уникальный идентификатор удаляемого пользователя.</param>
public record DeleteUserCommand(Guid Id) : IRequest;

/// <summary>
/// Обработчик команды <see cref="DeleteUserCommand"/>.
/// </summary>
public class DeleteUserCommandHandler
    : IRequestHandler<DeleteUserCommand>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="DeleteUserCommandHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    public DeleteUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Обрабатывает команду удаления пользователя.
    /// </summary>
    /// <param name="request">Команда с идентификатором удаляемого пользователя.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если пользователь с указанным идентификатором не найден.
    /// </exception>
    public async Task Handle(
        DeleteUserCommand request,
        CancellationToken ct)
    {
        var user = await _context.Users
            .FirstAsync(x => x.Id == request.Id, ct);

        _context.Users.Remove(user);

        await _context.SaveChangesAsync(ct);
    }
}