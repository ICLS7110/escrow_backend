namespace Escrow.Api.Application.Features.Commands;

using Escrow.Api.Application.DTOs;

public record DeleteBankCommand(int Id) : IRequest<Result<string>>;
