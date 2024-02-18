using Accounts.Application.Common.Interfaces;
using Accounts.Application.Common.Models;
using Accounts.Application.Member.Commands.UpdateMember.v1;
using Accounts.Domain.Enums;
using Accounts.Domain.Events;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Accounts.Application.Member.Handlers
{
    public class MemberCredentialChangedEventHandler : IConsumer<DomainEventNotification<MemberCredentialChanged>>
    {
        private readonly ILogger<MemberCredentialChangedEventHandler> _logger;
        private readonly IMediator _mediator;
        private readonly IEncryptionService _encryptionService;
        private PIIData _piiData;

        public MemberCredentialChangedEventHandler(ILogger<MemberCredentialChangedEventHandler> logger,
                                              IMediator mediator,
                                              IEncryptionService encryptionService)
        {
            _logger = logger;
            _mediator = mediator;
            _encryptionService = encryptionService;
        }

        public Task Consume(ConsumeContext<DomainEventNotification<MemberCredentialChanged>> context)
        {
            var domainEvent = context.Message.DomainEvent;
            _logger.LogInformation($"Member Credential changed: cognitoId = {domainEvent.CognitoId}");

            // Decrpyt the PII data to get email or phone
            if (!string.IsNullOrEmpty(domainEvent.PIIData))
            {
                try
                {
                    string piiData = _encryptionService.Decrypt(domainEvent.PIIData, EncryptionDataType.Base64, domainEvent.PIIIEncryptAlgorithm).Result.ToString();
                    _piiData = JsonConvert.DeserializeObject<PIIData>(piiData);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "PasswordChangedEventHandler, CognitoId={CognitoId}", domainEvent.CognitoId);
                }
            }

            // use mediator to send UpdateMemberCommand to update member detail in the database
            if (!string.IsNullOrEmpty(_piiData?.Email) || !string.IsNullOrEmpty(_piiData?.Phone))
            {
                UpdateMemberCommand updateCommand = new UpdateMemberCommand()
                {
                    CognitoId = domainEvent.CognitoId,
                    Email = _piiData?.Email,
                    Mobile = _piiData?.Phone
                };

                _mediator.Publish<UpdateMemberCommand>(updateCommand);
            }

            return Task.CompletedTask;
        }
    }
}