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
public class ExecutePaymentCommand : IRequest<Result<ExecutePaymentResultDto>>
{
    public int PaymentMethodId { get; set; }
    public decimal InvoiceAmount { get; set; }
    public string CurrencyIso { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerMobile { get; set; } = string.Empty;
}


public class ExecutePaymentHandler : IRequestHandler<ExecutePaymentCommand, Result<ExecutePaymentResultDto>>
{
    private readonly IMyFatoorahService _paymentService;

    public ExecutePaymentHandler(IMyFatoorahService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<Result<ExecutePaymentResultDto>> Handle(ExecutePaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _paymentService.ExecutePaymentAsync(request);
            return Result<ExecutePaymentResultDto>.Success(StatusCodes.Status200OK, "Payment executed successfully.", response);
        }
        catch (Exception ex)
        {
            // Log the exception as needed here
            return Result<ExecutePaymentResultDto>.Failure(StatusCodes.Status500InternalServerError, $"Payment execution failed: {ex.Message}");
        }
    }

}
