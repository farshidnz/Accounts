using Accounts.Domain.Common;
using System.Collections.Generic;

namespace Accounts.Infrastructure.AWS
{
    public interface IAWSEventServiceFactory
    {
        List<IAWSEventService> GetAWSPublishersForEvent(DomainEvent domainEvent);

        List<IAWSEventService> GetAWSEventReaders(AwsEventReadMode readMode);
    }
}