using Escrow.Api.Application.Authentication.Interfaces;
using System;

namespace Escrow.Api.Infrastructure.Authentication.Services
{
    public class SimpleOtpService : IOtpService
    {
        public Task<string> GenerateOtpAsync()
        {
           /* var random = new Random();
            return Task.FromResult(random.Next(100000, 999999).ToString());*/
            // Return a static OTP for consistent testing
            return Task.FromResult("1234");
        }

        public Task SendOtpAsync(string phoneNumber, string otp)
        {
            // Implement your custom logic here for sending the OTP (e.g., via SMS, email, etc.)
            Console.WriteLine($"OTP sent to {phoneNumber}: {otp}");
            return Task.CompletedTask;
        }

    }
}
