using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models.Payments;
public class PaymentMethodDto
{
    public int PaymentMethodId { get; set; }
    public string PaymentMethodAr { get; set; } = string.Empty;
    public string PaymentMethodEn { get; set; } = string.Empty;
    public string PaymentMethodCode { get; set; } = string.Empty;
    public bool IsDirectPayment { get; set; }
    public double ServiceCharge { get; set; }
    public double TotalAmount { get; set; }
    public string CurrencyIso { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsEmbeddedSupported { get; set; }
    public string PaymentCurrencyIso { get; set; } = string.Empty;
}
