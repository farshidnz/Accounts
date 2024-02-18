using Accounts.Infrastructure.Persistence;
using Moq;
using System.Collections.Generic;

namespace Accounts.Application.UnitTests.Member.Helpers;

public class MockIRepository : Mock<IRepository>
{
    private static List<Domain.Entities.Member> membersTestData => TestDataLoader.Load<List<Domain.Entities.Member>>("../../../Member/Json/MemberList.json");

    private static (Domain.Entities.Member, List<Domain.Entities.Client>) memberClientTestData
    {
        get
        {
            return
             (TestDataLoader.Load<Domain.Entities.Member>("../../../Member/Json/MemberListClient.json"), TestDataLoader.Load<List<Domain.Entities.Client>>("../../../Member/Json/ClientList.json"));
        }
    }

    private static Domain.Entities.Member memberTestData => TestDataLoader.Load<Domain.Entities.Member>("../../../Member/Json/MemberListClient.json");

    public MockIRepository()
    {
        Setup(r => r.QueryAsync<Domain.Entities.Member>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<int>()))
            .ReturnsAsync(membersTestData);
        
        Setup(r => r.ExecuteAsync(It.IsAny<string>(),It.IsAny<object>(),It.IsAny<int>()));
    }
}