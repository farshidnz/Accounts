using Accounts.Domain.Common;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace Accounts.API.Controllers;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
public abstract class BaseController : ControllerBase
{
    private const string TOKEN_CLIENT_ID_CLAIM_NAME = "client_id";
    private const string TOKEN_USERNAME_CLAIM_NAME = "username";

    private UserToken _userToken { get; set; }

    private string _token;

    private string Token
    {
        get
        {
            if (string.IsNullOrEmpty(_token))
                _token = HttpContextAccessor.HttpContext.Request.Headers[HeaderNames.Authorization];

            return _token;
        }
    }

    internal UserToken UserToken
    {
        get
        {
            if (!string.IsNullOrEmpty(Token) && _userToken == null)
            {
                _userToken = new();

                var jwt = Token.ToString().Replace("Bearer", string.Empty).Trim();

                var token = new JwtSecurityToken(jwt);

                _userToken.ClientId = token.Claims.FirstOrDefault(c => c.Type == TOKEN_CLIENT_ID_CLAIM_NAME)?.Value;
                _userToken.CognitoId = token.Claims.FirstOrDefault(c => c.Type == TOKEN_USERNAME_CLAIM_NAME)?.Value;
                _userToken.AccessToken = jwt.ToString();
            }
            return _userToken;
        }
    }

    private IMediator _mediator;
    private IHttpContextAccessor _httpContextAccessor;

    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();
    protected IHttpContextAccessor HttpContextAccessor => _httpContextAccessor ??= HttpContext.RequestServices.GetService<IHttpContextAccessor>();
}