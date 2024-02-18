using Accounts.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Accounts.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId => CognitoId;

    public string CognitoId => _httpContextAccessor.HttpContext?.User?.FindFirst("username")?.Value;
}