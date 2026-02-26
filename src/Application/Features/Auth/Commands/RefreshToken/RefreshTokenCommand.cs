using MediatR;
using Application.Features.Auth.DTO;

namespace Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<AuthResponse>;