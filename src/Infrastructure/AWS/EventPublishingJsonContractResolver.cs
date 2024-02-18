using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Infrastructure.AWS
{
    public class EventPublishingJsonContractResolver : DefaultContractResolver
    {
        private readonly HashSet<string> propNamesToIgnore;

        public EventPublishingJsonContractResolver(IEnumerable<string> propNamesToIgnore)
        {
            this.propNamesToIgnore = new HashSet<string>(propNamesToIgnore);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (propNamesToIgnore.Contains(property.PropertyName))
            {
                property.ShouldSerialize = (x) => { return false; };
            }

            return property;
        }
    }
}