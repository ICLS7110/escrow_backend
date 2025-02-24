namespace Escrow.Api.Application.DTOs;

public record BankDto
{
    public int Id { get; set; }
    public string AccountHolderName { get; set; } = string.Empty;
    public string IBANNumber { get; set; } = string.Empty;
    public string BICCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
}
