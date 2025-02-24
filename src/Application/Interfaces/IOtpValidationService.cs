namespace Escrow.Api.Application.Interfaces;
public interface IOtpValidationService
{
    Task<bool> ValidatePhoneNumberAsync(string phoneNumber);
}
