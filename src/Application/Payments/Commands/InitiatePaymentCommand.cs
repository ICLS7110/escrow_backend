using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.Payments;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.Payments.Commands;
public class InitiatePaymentCommand : IRequest<Result<List<PaymentMethodDto>>>
{
    public decimal InvoiceAmount { get; set; }
    public string CurrencyIso { get; set; } = string.Empty;
}
public class InitiatePaymentHandler : IRequestHandler<InitiatePaymentCommand, Result<List<PaymentMethodDto>>>
{
    private readonly IMyFatoorahService _paymentService;

    public InitiatePaymentHandler(IMyFatoorahService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<Result<List<PaymentMethodDto>>> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var methods = await _paymentService.InitiatePaymentAsync(request.InvoiceAmount, request.CurrencyIso);

            return Result<List<PaymentMethodDto>>.Success(
                StatusCodes.Status200OK,
                "Available payment methods retrieved.",
                methods
            );
        }
        catch (Exception ex)
        {
            // You can log the exception here if needed (e.g., using Serilog, ILogger, etc.)

            return Result<List<PaymentMethodDto>>.Failure(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while retrieving payment methods: {ex.Message}"
            );
        }
    }
}

