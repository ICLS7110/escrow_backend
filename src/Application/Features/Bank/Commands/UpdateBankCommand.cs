namespace Escrow.Api.Application.Features.Commands;

using Escrow.Api.Application.DTOs;

public class UpdateBankCommand : IRequest<Result<BankDto>>
{
    public int Id { get; set; } 
    public string AccountHolderName { get; set; } = string.Empty;
    public string IBANNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string? BICCode { get; set; } 

}
