namespace Escrow.Api.Application.Interfaces;
public interface IOtpService
{
    Task<string> GenerateOtpAsync();
    Task SendOtpAsync(string phoneNumber, string otp);
}
