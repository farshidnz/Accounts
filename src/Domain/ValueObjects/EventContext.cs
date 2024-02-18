using Accounts.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Accounts.Domain.ValueObjects
{
    public class EventContext : ValueObject
    {
        public HashSet<object> EventContextPropertyValues { get; }

        public EventContext(IEnumerable<object> eventContextPropertyValues)
        {
            this.EventContextPropertyValues = eventContextPropertyValues.ToHashSet();
        }

        public override string ToString() => $"[{String.Join(", ", EventContextPropertyValues)}]";

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var other = (EventContext)obj;

            return EventContextPropertyValues.SetEquals(other.EventContextPropertyValues);
        }

        public override int GetHashCode() => base.GetHashCode();

        protected override IEnumerable<object> GetEqualityComponents()
        {
            foreach (var eventContextPropertyValue in EventContextPropertyValues)
            {
                yield return eventContextPropertyValue;
            }
        }
    }
}