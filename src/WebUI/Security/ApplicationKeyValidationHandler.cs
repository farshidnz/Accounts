using Accounts.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Accounts.API.Security
{
    public class ApplicationKeyValidationRequirement : IAuthorizationRequirement
    {
    }

    public class ApplicationKeyValidationHandler : AuthorizationHandler<ApplicationKeyValidationRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApplicationKeyValidation _ApplicationKeyValidationService;
        private static string CR_APPLICATION_HEADER = "Cr-Application-Key";

        public ApplicationKeyValidationHandler(IHttpContextAccessor httpContextAccessor, IApplicationKeyValidation crApplicationKeyValidationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _ApplicationKeyValidationService = crApplicationKeyValidationService;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApplicationKeyValidationRequirement requirement)
        {
            #if DEBUG
            context.Succeed(requirement);
            return Task.CompletedTask;
            #endif

            if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey(CR_APPLICATION_HEADER) &&
                _ApplicationKeyValidationService.IsValid(_httpContextAccessor.HttpContext.Request.Headers[CR_APPLICATION_HEADER]))
            {
                context.Succeed(requirement);
            }
            else
                context.Fail();

            return Task.CompletedTask;
        }
    }
}