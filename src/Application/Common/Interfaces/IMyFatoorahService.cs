using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Models.Payments;
using Escrow.Api.Application.Payments.Commands;

namespace Escrow.Api.Application.Common.Interfaces;
public interface IMyFatoorahService
{
    Task<List<PaymentMethodDto>> InitiatePaymentAsync(decimal invoiceAmount, string currencyIso);
    Task<ExecutePaymentResultDto> ExecutePaymentAsync(ExecutePaymentCommand command);
}
