using Accounts.Infrastructure.AWS;
using System.Linq;

namespace Accounts.Infrastructure.Services
{
    public interface IApplicationKeyValidation
    {
        bool IsValid(string key);
    }

    public class ApplicationKeyValidationService : IApplicationKeyValidation
    {
        private readonly ISimpleSystemManagementService _managementService;
        private readonly char APPLICATION_KEY_SEPERATOR = ',';
        private const string MAIN_SITE_API_KEY_NAME = "AWS:MainSiteApiKeyName";

        public ApplicationKeyValidationService(ISimpleSystemManagementService managementService)
        {
            _managementService = managementService;
        }

        public bool IsValid(string key)
        {
            var systemKeys = _managementService.GetParameter(MAIN_SITE_API_KEY_NAME).Key?.Split(APPLICATION_KEY_SEPERATOR);
            return key.Split(APPLICATION_KEY_SEPERATOR).Intersect(systemKeys!).Any();
        }
    }
}