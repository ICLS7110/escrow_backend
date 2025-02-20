using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Escrow.Api.Infrastructure.Security;
public class RsaHelper : IRsaHelper
{
    private readonly RSA _rsa;    
    public RsaHelper(IConfiguration configuration)
    {      
        _rsa = RSA.Create();        
        var privatekeyvalue = configuration["RSA:privateKey"];
        _rsa.ImportFromPem(privatekeyvalue);
        
    }
    public string EncryptWithPrivateKey(string data)
    {
        byte[] encryptedBytes = _rsa.Encrypt(Encoding.UTF8.GetBytes(data), RSAEncryptionPadding.Pkcs1);
        return Convert.ToBase64String(encryptedBytes);
    }

    public string DecryptWithPrivateKey(string encryptedData)
    {
        byte[] decryptedBytes = _rsa.Decrypt(Convert.FromBase64String(encryptedData), RSAEncryptionPadding.Pkcs1);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
