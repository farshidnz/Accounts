using Accounts.Application.Common.Mappings;
using Mapster;

namespace Accounts.Application.Member.Commands.LinkCognitoId;

using Accounts.Domain.Events;
using Domain.Entities;
using System;

public class LinkCognitoIdCommand : IMapTo<MemberCognito>, IMapTo<CognitoLinked>
{
    public string Email { get; set; }

    public Guid? CognitoId { get; set; }

    // This is the CognitoPoolId from PasswordChanged domain event( from AWS cognito in Accounts API)
    // It's stored in the cognitopool table as cognitopoolname
    public string CognitoPoolId { get; set; }

    public string PIIData { get; set; } = "";
#nullable enable
    public string? PIIIEncryptAlgorithm { get; set; }

    public void Mapping(TypeAdapterConfig config)
    {
        config.ForType<(LinkCognitoIdCommand, MemberCognito), MemberCognito>()
                .Map(dest => dest.CognitoId, src => src.Item1.CognitoId)
                .Map(dest => dest.CognitoPoolId, src => src.Item2.CognitoPoolId)
                .Map(dest => dest.Email, src => src.Item1.Email)
                .Map(dest => dest.PIIData, src => src.Item1.PIIData)
                .Map(dest => dest.PIIIEncryptAlgorithm, src => src.Item1.PIIIEncryptAlgorithm)
                .Map(dest => dest.DomainName, _ => "Member");

        config.ForType<LinkCognitoIdCommand, CognitoLinked>()
               .Map(dest => dest.CognitoId, src => src.CognitoId)
               .Map(dest => dest.CognitoPoolId, src => src.CognitoPoolId)
               .Map(dest => dest.PIIData, src => src.PIIData)
               .Map(dest => dest.PIIIEncryptAlgorithm, src => src.PIIIEncryptAlgorithm);
    }
}

