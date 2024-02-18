using Accounts.Application.Common.Interfaces;
using Accounts.Domain.Enums;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Infrastructure.AWS;

public class KMSEncryptionService : IEncryptionService
{
    private readonly AmazonKeyManagementServiceClient _client;
    private readonly ILogger<KMSEncryptionService> _logger;
    private readonly string _kmsKey;

    private const string EncryptAlgorithm = "SYMMETRIC_DEFAULT";

    public KMSEncryptionService(IConfiguration configuration, ILogger<KMSEncryptionService> logger, AmazonKeyManagementServiceClient client)
    {
        var _configuration = configuration;
        _logger = logger;
        _client = client;
        var keyArn = _configuration["AccountDomainPIIKeyArn"];
        _kmsKey = GetKeyFromArn(keyArn);
        if (string.IsNullOrEmpty(_kmsKey))
        {
            _logger.LogError("KMSEncryptionService: AccountDomainPIIKeyArn - {keyArn} is invalid!!!", keyArn);
        }
    }

    private static string GetKeyFromArn(string arn)
    {
        if (string.IsNullOrEmpty(arn))
        {
            return null;
        }

        var arr = arn.Split(":key/");
        return arr.Length > 1 ? arr[1] : null;
    }

    // It's used a sysymmetric KMS key to encrypt/decrypt the data which size is less then 4K, the key is included in the metadata,
    // for the decrypt function, no need to provide the keyId.

    /// <summary>
    /// Decrypts string information into base64 string or string
    /// </summary>
    /// <param name="encryptedData">data to be decrypted</param>
    /// <param name="dataType">String or Base64</param>
    /// <param name="encryptAlgorithm"></param>
    /// <returns>String</returns>
    public async Task<string> Decrypt(string encryptedData, EncryptionDataType dataType = EncryptionDataType.String, string encryptAlgorithm = EncryptAlgorithm)
    {
        string decryptedValue = string.Empty;
        if (!string.IsNullOrEmpty(encryptedData))
        {
            try
            {
                DecryptRequest decryptRequest = new();
                decryptRequest.EncryptionAlgorithm = encryptAlgorithm;

                var textBytes = dataType == EncryptionDataType.String ? Encoding.UTF8.GetBytes(encryptedData)
                    : Convert.FromBase64String(encryptedData); ;
                decryptRequest.CiphertextBlob = new System.IO.MemoryStream(textBytes, 0, textBytes.Length);
                var response = await _client.DecryptAsync(decryptRequest);
                if (response != null)
                {
                    decryptedValue = Encoding.UTF8.GetString(response.Plaintext.ToArray());
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"KMSEncryptionService:DecryptToStringFrom{dataType} - fail to decrypt the data:{encryptedData}", encryptedData);
            }
        }
        return decryptedValue;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="toEncrypt">string to ge encrypted</param>
    /// <param name="DataType">the type of encryption , base 64 or string</param>
    /// <param name="encryptAlgorithm">the encrtypt alghoritm</param>
    /// <returns>String</returns>
    public async Task<string> Encrypt(string toEncrypt, EncryptionDataType dataType = EncryptionDataType.String, string encryptAlgorithm = EncryptAlgorithm)
    {
        string encryptedValue = string.Empty;
        if (!string.IsNullOrEmpty(toEncrypt) || _kmsKey == null)
        {
            try
            {
                EncryptRequest encryptRequest = new();
                encryptRequest.KeyId = _kmsKey;
                encryptRequest.EncryptionAlgorithm = encryptAlgorithm;

                var textBytes = Encoding.UTF8.GetBytes(toEncrypt);
                encryptRequest.Plaintext = new System.IO.MemoryStream(textBytes, 0, textBytes.Length);
                var response = await _client.EncryptAsync(encryptRequest);
                if (response != null)
                {
                    encryptedValue = dataType == EncryptionDataType.Base64 ? Convert.ToBase64String(response.CiphertextBlob.ToArray()) :
                    Encoding.UTF8.GetString(response.CiphertextBlob.ToArray());
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"KMSEncryptionService:EncryptStringFrom{dataType} - fail to encrypt the string:{toEncrypt}", toEncrypt);
            }
        }
        return encryptedValue;
    }
}