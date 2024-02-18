using Accounts.Application.Common.Extensions;
using System;

namespace Accounts.Infrastructure.AWS
{
    public enum AwsResourceType
    {
        Unknown,
        SNS,
        SQS
    }

    public enum AwsEventReadMode
    {
        Unknown,
        LambdaTrigger,
        PolledRead
    }

    public abstract class AWSEventResource
    {
        public string Type { get; set; }

        public AwsResourceType AWSResourceType
        {
            get
            {
                return Type.ToEnum<AwsResourceType>();
            }
        }

        public string Domain { get; set; }
        public Type EventType { get; set; }
        public string EventTypeName { get; set; }
    }

    public class AWSEventSource : AWSEventResource
    {
        public string ReadMode { get; set; }

        public AwsEventReadMode AWSReadMode
        {
            get
            {
                return ReadMode.ToEnum<AwsEventReadMode>();
            }
        }
    }

    public class AWSEventDestination : AWSEventResource
    {
    }
}