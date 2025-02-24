
namespace Escrow.Api.Application.Features.Queries;

using Escrow.Api.Application.DTOs;

public record GetBankByIdQuery : IRequest<Result<BankDto>>
{
    public string? Id { get; init; }
}
