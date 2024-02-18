using System;

namespace Accounts.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EventContextProvider : Attribute
    { }

    [AttributeUsage(AttributeTargets.Class)]
    public class EventContextName : Attribute
    {
        public EventContextName(string EventContextName)
        {
            this.ContextName = EventContextName;
        }

        public string ContextName { get; }
    }
}