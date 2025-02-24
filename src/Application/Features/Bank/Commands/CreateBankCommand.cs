namespace Escrow.Api.Application.Features.Commands;

using Escrow.Api.Application.DTOs;

public record CreateBankCommand : IRequest<Result<BankDto>>
{
    public string AccountHolderName { get; set; } = string.Empty;
    public string IBANNumber { get; set; } = string.Empty;
    public string BICCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
}
