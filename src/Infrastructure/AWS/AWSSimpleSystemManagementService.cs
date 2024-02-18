    using Accounts.Domain.Common;
    using Amazon.SimpleSystemsManagement;
    using Amazon.SimpleSystemsManagement.Model;
    using Microsoft.Extensions.Configuration;

namespace Accounts.Infrastructure.AWS
{
    public interface ISimpleSystemManagementService
    {
        CrApplication GetParameter(string parameterName);
    }

    public class AwsSimpleSystemManagementService : ISimpleSystemManagementService
    {
        private readonly IConfiguration _configuration;

        public AwsSimpleSystemManagementService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public virtual CrApplication GetParameter(string parameterName)
        {
            var client = new AmazonSimpleSystemsManagementClient();

            var parameter = new GetParameterRequest { Name = _configuration[parameterName], WithDecryption = true };

            var resultResponse = client.GetParameterAsync(parameter).ConfigureAwait(false).GetAwaiter().GetResult();
            var key = resultResponse?.Parameter?.Value;
            return new CrApplication(key);
        }
    }
}