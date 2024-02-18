using Accounts.Application.Common.Interfaces;
using Moq;

namespace Accounts.Application.UnitTests.Member.Helpers;

public class MockIEncryptionService : Mock<IEncryptionService>
{
    public MockIEncryptionService()
    {
        Setup(r => r.Encrypt(It.IsAny<string>(), default, default));

        Setup(r => r.Decrypt(It.IsAny<string>(), default, default));
    }
}