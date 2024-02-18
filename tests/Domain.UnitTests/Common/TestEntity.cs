using Accounts.Domain.Common;
using System.Collections.Generic;

namespace Accounts.Domain.UnitTests.Common
{
    public class TestEntityId : ValueObject
    {
        private readonly string id;

        public TestEntityId(string id)
        {
            this.id = id;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return id;
        }
    }

    public class TestEntity : DomainEntity, IHasIdentity<string>
    {
        public TestEntity(string iD)
        {
            ID = iD;
        }

        public TestEntityId id { get; }

        public string ID { get; set; }

        public void DoSomethingThatRaisesEvent(string stateData)
        {
            RaiseEvent(new TestEvent() { state = stateData });
        }
    }
}