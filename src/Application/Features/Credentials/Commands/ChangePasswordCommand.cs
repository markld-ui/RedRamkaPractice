using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Credentials.Commands;

public class ChangePasswordCommandHandler
    : IRequestHandler<ChangePasswordCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _hasher;
    private readonly ICurrentUserService _currentUser;

    public ChangePasswordCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher hasher,
        ICurrentUserService currentUser)
    {
        _context = context;
        _hasher = hasher;
        _currentUser = currentUser;
    }

    public async Task Handle(
        ChangePasswordCommand request,
        CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var credentials = await _context.Credentials
            .FirstAsync(x => x.UserId == _currentUser.UserId, ct);

        if (!_hasher.Verify(
                request.CurrentPassword,
                credentials.PasswordHash))
            throw new UnauthorizedAccessException();

        credentials.PasswordHash =
            _hasher.Hash(request.NewPassword);

        credentials.RefreshToken = null;

        await _context.SaveChangesAsync(ct);
    }
}
public record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword) : IRequest;
