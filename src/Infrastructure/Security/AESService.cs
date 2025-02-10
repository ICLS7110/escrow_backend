using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Infrastructure.OptionConfiguration;
using Microsoft.Extensions.Options;

namespace Escrow.Api.Infrastructure.Security;
public class AESService : IAESService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public AESService(IOptions<AESSettings> aesSettings)
    {
        _key = Convert.FromBase64String(aesSettings.Value.Key);
        _iv = Convert.FromBase64String(aesSettings.Value.IV);
    }
    public string Encrypt(string data)
    {
        using Aes aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        byte[] plainBytes = Encoding.UTF8.GetBytes(data);
        byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String(encryptedBytes);
    }

    public string Decrypt(string encryptedData)
    {
        using Aes aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
        byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
