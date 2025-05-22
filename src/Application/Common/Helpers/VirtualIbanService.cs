using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Helpers;
public class VirtualIbanService
{
    private readonly int _companyId;
    private readonly string _bankCode;

    public VirtualIbanService(int companyId, string bankCode)
    {
        _companyId = companyId;
        _bankCode = bankCode;
    }

    public string GenerateVirtualIban(long customerId)
    {
        var subAccount = GenerateSubAccountNumber(_companyId, customerId);
        return GenerateSaudiIban(_bankCode, subAccount);
    }

    private string GenerateSubAccountNumber(int companyId, long customerId)
    {
        string companyIdStr = companyId.ToString("D3");
        string customerIdStr = customerId.ToString().PadLeft(11, '0');
        string first14Digits = companyIdStr + customerIdStr;

        int checkDigit = CalculateCheckDigit(first14Digits);
        return first14Digits + checkDigit;
    }

    private int CalculateCheckDigit(string number14)
    {
        if (number14.Length != 14 || !long.TryParse(number14, out _))
            throw new ArgumentException("Input must be a 14-digit numeric string.");

        int oddSum = 0;
        int evenSum = 0;

        for (int i = 0; i < 14; i++)
        {
            int digit = number14[i] - '0';
            if ((i + 1) % 2 == 1)
                oddSum += digit;
            else
                evenSum += digit;
        }

        int total = (oddSum * 3) + evenSum;
        int rightMostDigit = total % 10;
        return 9 - rightMostDigit;
    }

    private string GenerateSaudiIban(string bankCode, string accountNumber)
    {
        string bban = bankCode + accountNumber;
        string rearranged = bban + "SA00";
        string numericIban = ConvertToNumericString(rearranged);

        BigInteger ibanNumber = BigInteger.Parse(numericIban);
        int mod97 = (int)(ibanNumber % 97);
        int checkDigits = 98 - mod97;

        return $"SA{checkDigits:D2}{bban}";
    }

    private string ConvertToNumericString(string input)
    {
        var numeric = new StringBuilder();
        foreach (char c in input)
        {
            if (char.IsLetter(c))
                numeric.Append((c - 'A' + 10).ToString());
            else
                numeric.Append(c);
        }
        return numeric.ToString();
    }





    /// <summary>
    /// This method is used to generate a sub-account number based on the company ID and customer ID.
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="customerId"></param>
    /// <returns></returns>
    public string GetSubAccountNumber(int companyId, long customerId)
    {
        return GenerateSubAccountNumber(companyId, customerId);
    }


}

