using Accounts.Application.Common.Interfaces;
using Accounts.Application.Common.Models;
using Accounts.Application.Member.Commands.LinkCognitoId;
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
    public class PasswordChangedEventHandler : IConsumer<DomainEventNotification<PasswordChanged>>
    {
        private readonly ILogger<PasswordChangedEventHandler> _logger;
        private readonly IMediator _mediator;
        private readonly IEncryptionService _encryptionService;
        private PIIData _piiData;

        public PasswordChangedEventHandler(ILogger<PasswordChangedEventHandler> logger,
                                      IMediator mediator,
                                      IEncryptionService encryptionService)
        {
            _logger = logger;
            _mediator = mediator;
            _encryptionService = encryptionService;
        }

        public Task Consume(ConsumeContext<DomainEventNotification<PasswordChanged>> context)
        {
            PasswordChanged domainEvent = context.Message.DomainEvent;
            _logger.LogInformation($"Member Password changed: cognitoId = {domainEvent.CognitoId}");
            // Decrpyt the PII data to get email
            if (!string.IsNullOrEmpty(domainEvent.PIIData))
            {
                try
                {
                    var piiDataString = _encryptionService.Decrypt(domainEvent.PIIData, EncryptionDataType.Base64, domainEvent.PIIIEncryptAlgorithm).Result.ToString();
                    _piiData = JsonConvert.DeserializeObject<PIIData>(piiDataString);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "PasswordChangedEventHandler, CognitoId={CognitoId} Email={Email}", domainEvent.CognitoId, _piiData?.Email);
                }
            }

            // use mediator to send LinkCognitoIdCommand to link the cognitoId with memberId with the email in the database
            if (!string.IsNullOrEmpty(_piiData?.Email))
            {
                LinkCognitoIdCommand command = new LinkCognitoIdCommand()
                {
                    CognitoId = domainEvent.CognitoId,
                    CognitoPoolId = domainEvent.CognitoPoolId,
                    PIIData = domainEvent.PIIData,
                    PIIIEncryptAlgorithm = domainEvent.PIIIEncryptAlgorithm,
                    Email = _piiData.Email
                };

                _mediator.Publish<LinkCognitoIdCommand>(command);
            }

            return Task.CompletedTask;
        }
    }
}