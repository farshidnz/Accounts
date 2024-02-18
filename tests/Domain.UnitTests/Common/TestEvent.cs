using Accounts.Domain.Common;

namespace Accounts.Domain.UnitTests.Common
{
    public class TestEvent : DomainEvent
    {
        public string state { get; set; }
    }
}