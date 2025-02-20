using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application;
public static class EscrowApiExtensions
{
    public static int ToInt(this string value, int defaultValue = 0)
    {
        return int.TryParse(value, out int result) ? result : defaultValue;
    }


}
