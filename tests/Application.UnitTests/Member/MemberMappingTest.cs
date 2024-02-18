using Accounts.Domain.Entities;
using System;

namespace Accounts.Application.UnitTests.Member;

public class MemberMappingTest
{
    private static MemberContext MemberContext => new()
    {
        CognitoId = Guid.Parse("a34d8785-f34b-42fc-ab0b-36f0b73dc746"),
        Email = "test@cashrewards.com"
    };
}