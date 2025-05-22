using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.Common.Models.Payments;

namespace Escrow.Api.Application.Common.Interfaces;
public interface IAnbService
{
    Task<string> GetAccountVerificationStatusAsync(string requestReferenceNumber);
    Task<VerifyAccountResponse?> VerifyAccountAsync(VerifyAccountRequest request);

    Task<string?> GetAccessTokenAsync();

    Task<SanctionsCheckResponse?> CheckSanctionsAsync(string nationalId);
    Task<PaymentResponse?> MakeBankToBankTransferAsync(PaymentRequest request);
    Task<PaymentStatusResponse?> GetPaymentStatusAsync(string paymentId);
}
