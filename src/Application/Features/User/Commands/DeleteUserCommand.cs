namespace Escrow.Api.Application.Features.Commands;

using Escrow.Api.Application.DTOs;

public record DeleteUserCommand(int Id) : IRequest<Result<string>>;



