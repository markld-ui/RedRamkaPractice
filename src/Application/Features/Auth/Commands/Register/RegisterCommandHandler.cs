using Application.Common.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.Register;

/// <summary>
/// Обработчик команды <see cref="RegisterCommand"/>.
/// </summary>
public class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _hasher;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="RegisterCommandHandler"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения.</param>
    /// <param name="hasher">Сервис хэширования паролей.</param>
    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    /// <summary>
    /// Обрабатывает команду регистрации нового пользователя.
    /// </summary>
    /// <param name="request">Команда, содержащая данные регистрируемого пользователя.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// Уникальный идентификатор созданного пользователя.
    /// </returns>
    /// <exception cref="Exception">
    /// Выбрасывается, если пользователь с указанным адресом электронной почты
    /// уже зарегистрирован в системе.
    /// </exception>
    public async Task<Guid> Handle(
        RegisterCommand request,
        CancellationToken ct)
    {
        var exist = await _context.Credentials
            .AnyAsync(x => x.Email == request.Email, ct);

        if (exist)
            throw new Exception("Email already exists");

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var credentials = new Domain.Models.Credentials
        {
            Email = request.Email,
            PasswordHash = _hasher.Hash(request.Password),
            User = user
        };

        user.Credentials = credentials;

        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);

        return user.Id;
    }
}