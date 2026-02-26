using MediatR;
using Application.Features.Auth.DTO;

namespace Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
    ) : IRequest<AuthResponse>;