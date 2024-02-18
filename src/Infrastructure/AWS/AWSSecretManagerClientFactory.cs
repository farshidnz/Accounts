using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Text.Json;
namespace Accounts.Infrastructure.AWS
{
    public interface IAwsSecretsManagerClientFactory
    {
        Task<T> GetSecretValueAsync<T>(string secretName);
    }

    public class AwsSecretsManagerClientFactory : IAwsSecretsManagerClientFactory
    {
        private readonly string _region;


        public AwsSecretsManagerClientFactory(IConfiguration configuration)
        {
            _region = configuration["AWS:Region"];
        }

        private IAmazonSecretsManager CreateClient() => new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(_region));

        public async Task<T> GetSecretValueAsync<T>(string secretName)
        {
            using var client = CreateClient();
            var response = await client.GetSecretValueAsync(new GetSecretValueRequest { SecretId = secretName });
            return JsonSerializer.Deserialize<T>(response?.SecretString);
        }
    }
}