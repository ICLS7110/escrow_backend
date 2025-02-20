using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Infrastructure.Security;
public interface IRsaHelper
{
    string EncryptWithPrivateKey(string data);
    string DecryptWithPrivateKey(string encryptedData);
}
