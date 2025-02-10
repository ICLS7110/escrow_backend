using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Interfaces;
public interface IAESService
{
    string Encrypt(string data);
    string Decrypt(string encryptedData);
}
