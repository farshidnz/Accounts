namespace Accounts.Infrastructure.Configuration;

public class AWSOptions
{
    public string Region { get; set; }
    public string UserPoolId { get; set; }
    public string MainSiteApiKeyName { get; set; }
}