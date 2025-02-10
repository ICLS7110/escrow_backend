using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Infrastructure.OptionConfiguration;
public class AESSettings
{
    public string Key { get; set; } = string.Empty;
    public string IV { get; set; } = string.Empty;
}


public class ConnectionStrings
{
    public string Escrow { get; set; } = string.Empty;
}
