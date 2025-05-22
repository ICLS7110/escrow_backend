namespace Escrow.Api.Web.UserEndpoints.BankDetails;

public class ReferenceRequest
{
    public char PartyIndicator { get; set; }  // 'S' or 'B'
    public int CustomerId { get; set; }
    public int ContractId { get; set; }
    public int? MilestoneId { get; set; }
}
