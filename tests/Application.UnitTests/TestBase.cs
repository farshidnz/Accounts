using Accounts.Application.Member.Handlers;
using Accounts.Application.ReportSubscription.Handlers;
using Accounts.Application.UnitTests.Member.Helpers;
using MassTransit.Mediator;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq.Expressions;

namespace Accounts.Application.UnitTests;

public class TestMessage
{ }

internal class TestState
{
    public MockIRepository _repository { get; set; }

    public MockIEncryptionService _encryptionService { get; }
    public Mock<IMediator> _mediator { get; }
    public MemberSignedUpEventHandler _MemberSignedUpEventHandler { get; }
    public MemberSignedInEventHandler _MemberSignedInEventHandler { get; }
    public MemberCredentialChangedEventHandler _MemberCredentialChangedEventHandler { get; }
    public PasswordChangedEventHandler _PasswordChangedEventHandler { get; }
    public ReportSubscriptionMessageEventHandler _ReportSubscriptionMessageEventHandler { get; }

    public TestState()
    {
        _repository = new MockIRepository();
        _mediator = new Mock<IMediator>();
        _encryptionService = new MockIEncryptionService();
        _MemberSignedUpEventHandler = new MemberSignedUpEventHandler(new Mock<ILogger<MemberSignedUpEventHandler>>().Object, _mediator.Object, _encryptionService.Object);
        _MemberSignedInEventHandler = new MemberSignedInEventHandler(new Mock<ILogger<MemberSignedInEventHandler>>().Object, _mediator.Object);
        _MemberCredentialChangedEventHandler = new MemberCredentialChangedEventHandler(new Mock<ILogger<MemberCredentialChangedEventHandler>>().Object, _mediator.Object, _encryptionService.Object);
        _PasswordChangedEventHandler = new PasswordChangedEventHandler(new Mock<ILogger<PasswordChangedEventHandler>>().Object, _mediator.Object, _encryptionService.Object);
        _ReportSubscriptionMessageEventHandler = new ReportSubscriptionMessageEventHandler(new Mock<ILogger<PasswordChangedEventHandler>>().Object, _mediator.Object);
    }
}

public class TestBase
{
    public Expression<Action<ILogger<T>>> CheckLogMesssageMatches<T>(LogLevel logLevel, string logMsg)
    {
        return x => x.Log(logLevel,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => string.Equals(logMsg, o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>());
    }
}