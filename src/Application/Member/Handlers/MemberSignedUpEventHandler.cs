using Accounts.Application.Common.Interfaces;
using Accounts.Application.Common.Models;
using Accounts.Domain.Enums;
using Accounts.Domain.Events;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Application.Member.Handlers
{
    public class MemberSignedUpEventHandler : IConsumer<DomainEventNotification<MemberSignedUp>>
    {
        private readonly ILogger<MemberSignedUpEventHandler> _logger;
        private readonly IMediator _mediator;
        private readonly IEncryptionService _encryptionService;
        private PIIData _piiData;

        public MemberSignedUpEventHandler(ILogger<MemberSignedUpEventHandler> logger,
                                          IMediator mediator,
                                          IEncryptionService encryptionService)
        {
            _logger = logger;
            _mediator = mediator;
            _encryptionService = encryptionService;
        }

        public Task Consume(ConsumeContext<DomainEventNotification<MemberSignedUp>> context)
        {
            var domainEvent = context.Message.DomainEvent;
            _logger.LogInformation($"Member Signed Up: cognitoId = {domainEvent.CognitoId}, PoolId ={domainEvent.CognitoPoolId},  OriginationSource = {domainEvent.OriginationSource}");

            // Decrpyt the PII data
            if (!string.IsNullOrEmpty(domainEvent.PIIData))
            {
                var piiDataString = _encryptionService.Decrypt(domainEvent.PIIData, EncryptionDataType.Base64, domainEvent.PIIIEncryptAlgorithm).Result.ToString();
                _piiData = JsonConvert.DeserializeObject<PIIData>(piiDataString);
            }

            // TODO: use mediator to send CreateMemberCommand to create new member in the database 


            return Task.CompletedTask;
        }
    }
}
