using Accounts.Domain.Enums;
using System.Threading.Tasks;

namespace Accounts.Application.Common.Interfaces
{
    public interface IEncryptionService
    {
        Task<string> Decrypt(string encryptedData, EncryptionDataType dataType, string encryptAlgorithm);

        Task<string> Encrypt(string toEncrypt, EncryptionDataType DataType, string encryptAlgorithm);
    }
}