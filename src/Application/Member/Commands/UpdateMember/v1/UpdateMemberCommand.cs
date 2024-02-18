using Accounts.Application.Common.Mappings;
using Mapster;

namespace Accounts.Application.Member.Commands.UpdateMember.v1;

using Accounts.Domain.Events;
using Domain.Entities;

public class UpdateMemberCommand : MemberCommand, IMapTo<MemberContext>, IMapTo<MemberInfo>, IMapTo<Member>, IMapTo<MemberDetailChanged>
{
    public void Mapping(TypeAdapterConfig config)
    {
        config.ForType<UpdateMemberCommand, MemberContext>()
            .Map(dest => dest.IsGetByCognitoId, _ => true)
            .Map(dest => dest.SmsConcent, src => src.SubscribeSMS)
            .Map(dest => dest.AppNotificationConsent, src => src.SubscribeAppNotifications)
            .Map(dest => dest.ReceiveNewsLetter, src => src.SubscribeNewsletters)
            .Map(dest => dest.CommsPromptShownCount, src => ValidateComms(src.CommsPromptAction));

        config.ForType<(Member, UpdateMemberCommand), MemberInfo>()
             .Map(dest => dest, src => src.Item1)
             .Map(dest => dest.FirstName, src => src.Item2.FirstName)
             .Map(dest => dest.LastLogon, src => src.Item2.LastLogon)
             .Map(dest => dest.LastName, src => src.Item2.LastName)
             .Map(dest => dest.Mobile, src => src.Item2.Mobile)
             .Map(dest => dest.CommsPromptShownCount, src => ValidateComms(src.Item2.CommsPromptAction));

        config.ForType<(UpdateMemberCommand, Member), Member>()
            .Map(dest => dest.MemberId, src => src.Item2.MemberId)
            .Map(dest => dest.FirstName, src => src.Item1.FirstName ?? src.Item2.FirstName)
            .Map(dest => dest.LastName, src => src.Item1.LastName ?? src.Item2.LastName)
            .Map(dest => dest.PostCode, src => src.Item1.PostCode ?? src.Item2.PostCode)
            .Map(dest => dest.Email, src => src.Item1.Email ?? src.Item2.Email)
            .Map(dest => dest.ReceiveNewsLetter, src => src.Item1.SubscribeNewsletters ?? src.Item2.ReceiveNewsLetter)
            .Map(dest => dest.SmsConsent, src => src.Item1.SubscribeSMS ?? src.Item2.SmsConsent)
            .Map(dest => dest.AppNotificationConsent, src => src.Item1.SubscribeAppNotifications ?? src.Item2.AppNotificationConsent)
            .Map(dest => dest.IsValidated, src => src.Item1.IsValidated ?? src.Item2.IsValidated)
            .Map(dest => dest.Comment, src => src.Item1.Comment ?? src.Item2.Comment)
            .Map(dest => dest.RiskDescription, src => src.Item1.RiskDescription ?? src.Item2.RiskDescription)
            .Map(dest => dest.KycStatusId, src => src.Item1.KycStatusId ?? src.Item2.KycStatusId)
            .Map(dest => dest.TrustScore, src => src.Item1.TrustScore ?? src.Item2.TrustScore)
            .Map(dest => dest.IsRisky, src => src.Item1.IsRisky ?? src.Item2.IsRisky)
            .Map(dest => dest.OriginationSource, src => src.Item1.OriginationSource ?? src.Item2.OriginationSource)
            .Map(dest => dest.InstallNotifier, src => src.Item1.InstallNotifier ?? src.Item2.InstallNotifier)
            .Map(dest => dest.CommsPromptShownCount, src => ValidateComms(src.Item1.CommsPromptAction))
            .Map(dest => dest.LastLogon, src => src.Item1.LastLogon ?? src.Item2.LastLogon)
            .Map(dest => dest.Mobile, src => src.Item1.Mobile ?? src.Item2.Mobile)
            .Map(dest => dest.CognitoId, src => src.Item1.CognitoId)
            .Map(dest => dest.DomainName, _ => "UpdateMemberEvent")
            .Map(dest => dest.HasDomainEvents, _ => true);

        config.ForType<UpdateMemberCommand, MemberDetailChanged>()
            .Map(dest => dest.ReceiveNewsletter, src => src.SubscribeNewsletters)
            .Map(dest => dest.SMSConsent, src => src.SubscribeSMS)
            .Map(dest => dest.AppNotificationConsent, src => src.SubscribeAppNotifications)
            .Map(dest => dest.IsValidated, src => src.IsValidated)
            .Map(dest => dest.Comment, src => src.Comment)
            .Map(dest => dest.TrustScore, src => src.TrustScore)
            .Map(dest => dest.IsRisky, src => src.IsRisky)
            .Map(dest => dest.OriginationSource, src => src.OriginationSource)
            .Map(dest => dest.InstallNotifier, src => src.InstallNotifier);
    }

    private static int? ValidateComms(string value)
    {
        return !string.IsNullOrEmpty(value) ? int.Parse(value) : null;
    }
}