using Accounts.Application.Common.Exceptions;
using Accounts.Application.Member;
using Accounts.Application.Member.Commands.UpdateMember.v1;
using Accounts.Application.Member.Commands.VerifyEmail;
using Accounts.Application.Member.Model;
using Accounts.Application.Member.Queries.GetMember.v1;
using Accounts.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using Accounts.Application.Member.Commands.CreateMember.v1;

namespace Accounts.API.Controllers.v1;

public class MemberController : BaseController
{
    [HttpGet]
#if (DEBUG == false)
    [Authorize]
#endif
    [Route("{cognito}")]
    [ProducesResponseType(typeof(MemberInfo), (int)HttpStatusCode.OK)]
    public async Task<MemberInfo> GetMemberByCognito([FromRoute] string cognito)
    {
        if (string.IsNullOrEmpty(UserToken.CognitoId) || string.IsNullOrEmpty(cognito) || !string.Equals(UserToken.CognitoId,cognito))
            throw new BadRequestException("Cognito Id is not Valid");

        return await Mediator.Query<GetMemberQuery, MemberInfo>(new GetMemberQuery
        {
            CognitoId = Guid.Parse(UserToken.CognitoId),
            Email = string.Empty
        });
    }

    [HttpPatch]
    [ProducesResponseType(typeof(MemberInfo), (int)HttpStatusCode.OK)]
    public async Task<MemberInfo> Patch([FromBody] UpdateMemberCommand updateCommand)
    {
        return await Mediator.Query<UpdateMemberCommand, MemberInfo>(updateCommand);
    }


    [HttpPatch("create")]
    [AllowAnonymous]
    
    [ProducesResponseType(typeof(MemberInfo), (int)HttpStatusCode.OK)]
    public async Task<MemberInfo> Create([FromBody] CreateMemberCommand command)
    {
        return await Mediator.Query<CreateMemberCommand, MemberInfo>(command);
    }

    [AllowAnonymous]
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(VerifyEmailDto), (int)HttpStatusCode.OK)]
    public async Task<VerifyEmailDto> VerifyEmail([FromBody] VerifyEmailCommand request)
    {
        return await Mediator.Query<VerifyEmailCommand, VerifyEmailDto>(request);
    }

    [HttpGet]
    [Authorize(Policy = "InternalPolicy")]
    [Route("internal/{email}")]
    [ProducesResponseType(typeof(MemberInfo), (int)HttpStatusCode.OK)]
    public async Task<MemberInfo> GetMemberByEmail([FromRoute] string email)
    {
        return await Mediator.Query<GetMemberQuery, MemberInfo>(new GetMemberQuery
        {
            CognitoId = null,
            Email = email
        });
    }
}