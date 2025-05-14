using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Enums;

namespace Escrow.Api.Application.Common.Helpers;

public static class ProfileCompletionHelper
{
    public static int Calculate(UserDetail user)
    {
        int earnedPoints = 0;
        string accountType = user.AccountType?.Trim().ToLower() ?? "personal";

        if (accountType == "personal")
        {
            // PERSONAL ACCOUNT — Total 100
            if (!string.IsNullOrWhiteSpace(user.FullName))
                earnedPoints += 15;

            if (!string.IsNullOrWhiteSpace(user.EmailAddress))
                earnedPoints += 15;

            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
                earnedPoints += 15;

            if (!string.IsNullOrWhiteSpace(user.Gender))
                earnedPoints += 10;

            if (user.DateOfBirth != null)
                earnedPoints += 10;

            if (!string.IsNullOrWhiteSpace(user.ProfilePicture))
                earnedPoints += 25;

            if (!string.IsNullOrWhiteSpace(user.AccountType))
                earnedPoints += 10;

            return earnedPoints;
        }
        else if (accountType == "business")
        {
            // BUSINESS ACCOUNT — Total 100 (60 general + 40 business-specific)
            if (!string.IsNullOrWhiteSpace(user.FullName))
                earnedPoints += 10;

            if (!string.IsNullOrWhiteSpace(user.EmailAddress))
                earnedPoints += 10;

            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
                earnedPoints += 10;

            if (!string.IsNullOrWhiteSpace(user.Gender))
                earnedPoints += 5;

            if (user.DateOfBirth != null)
                earnedPoints += 5;

            if (!string.IsNullOrWhiteSpace(user.ProfilePicture))
                earnedPoints += 15;

            if (!string.IsNullOrWhiteSpace(user.AccountType))
                earnedPoints += 5;

            // Business fields (total 40)
            if (!string.IsNullOrWhiteSpace(user.BusinessManagerName))
                earnedPoints += 16;

            if (!string.IsNullOrWhiteSpace(user.BusinessEmail))
                earnedPoints += 16;

            if (!string.IsNullOrWhiteSpace(user.VatId))
                earnedPoints += 8;

            //if (!string.IsNullOrWhiteSpace(user.BusinessProof))
            //    earnedPoints += 8;

            //if (!string.IsNullOrWhiteSpace(user.CompanyEmail))
            //    earnedPoints += 8;

            return earnedPoints;
        }

        return earnedPoints;
    }
}

public static class ConfidenceRateHelper
{
    /// <summary>
    /// Calculates the confidence rate for a user based on their contracts.
    /// </summary>
    /// <param name="userId">The user whose confidence rate is being calculated.</param>
    /// <param name="context">The database context to retrieve contract data.</param>
    /// <param name="excludedStatuses">List of contract statuses to be excluded from total contracts.</param>
    /// <returns>The confidence rate as a percentage.</returns>
    public static async Task<int> CalculateConfidenceRate(int userId, IApplicationDbContext context)
    {
        var excludedStatuses = new List<string> { nameof(ContractStatus.Draft), nameof(ContractStatus.Pending), nameof(ContractStatus.Rejected), nameof(ContractStatus.Expired) };
        // Fetch contracts for the given user, excluding specific statuses
        var contracts = await context.ContractDetails
            .AsNoTracking()
            .Where(c => (c.BuyerDetailsId == userId || c.SellerDetailsId == userId || c.CreatedBy == userId.ToString()) && !excludedStatuses.Contains(c.Status) && c.IsDeleted == false)
            .ToListAsync();

        int totalContracts = contracts.Count();
        int disputeContracts = contracts.Count(c => c.Status == "dispute");

        // Calculate confidence rate
        return CalculateConfidenceRateFromContracts(totalContracts, disputeContracts);
    }

    /// <summary>
    /// Helper method to calculate confidence rate from total and disputed contracts.
    /// </summary>
    /// <param name="totalContracts">Total number of contracts.</param>
    /// <param name="disputeContracts">Number of disputed contracts.</param>
    /// <returns>The confidence rate as a percentage.</returns>
    private static int CalculateConfidenceRateFromContracts(int totalContracts, int disputeContracts)
    {
        if (totalContracts <= 0)
            return 0;

        int successfulContracts = totalContracts - disputeContracts;
        double rate = (successfulContracts * 100.0) / totalContracts;

        return (int)Math.Round(rate); // Round to nearest whole number
    }
}
