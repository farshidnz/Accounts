using Accounts.Domain.Common;
using Accounts.Infrastructure.AWS;
using Accounts.Infrastructure.Exceptions;
using Accounts.Infrastructure.OutboxHandlers;
using Accounts.Infrastructure.OutboxMessages;
using Accounts.Infrastructure.Persistence;
using DotNetCore.CAP;
using FluentAssertions;
using MassTransit.Mediator;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Accounts.Application.UnitTests.Member.Cap;

[TestFixture]
internal class CapTest : TestBase
{
    [Test]
    public async Task ShouldBeCalled_Once()
    {
        var mockCapRepo = new Mock<IOutboxRepository>();
        mockCapRepo.Setup(m => m.SaveAndPublish<DomainEntity>(It.IsAny<string>(), It.IsAny<DomainEntity>(), It.IsAny<object>(),It.IsAny<int?>()))
            .ReturnsAsync(It.IsAny<int>());

        Domain.Entities.Member eventTest = new();
        await mockCapRepo.Object.SaveAndPublish("testquery", eventTest, null);
        mockCapRepo.Verify(c => c.SaveAndPublish(It.IsAny<string>(), It.IsAny<DomainEntity>(), It.IsAny<object>(), It.IsAny<int?>()), Times.Once);
    }

    [Test]
    public async Task ShouldBe_PublishOutbox()
    {
        var capPublisher = new Mock<ICapPublisher>();
        var outboxMessage = new OutboxMessage()
        {
            Metadata = new EventMetadata()
            {
                CorrelationID = Guid.NewGuid(),
                EventType = typeof(TempEvent).ToString()
            },
            Message = "Message",
        };

        await capPublisher.Object.PublishAsync("testquery", outboxMessage, new Dictionary<string, string>(),
            CancellationToken.None);
        capPublisher.Verify(
            c => c.PublishAsync(It.IsAny<string>(), It.IsAny<It.IsAnyType>(),
                It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public Task ShouldBe_nullType()
    {
        var capPublisher = new Mock<Assembly>();
        capPublisher.Setup(m => m.GetType(It.IsAny<string>()))
            .Returns((Type)null);

        var type = capPublisher.Object.GetType("type");
        type.Should().BeNull();
        return Task.CompletedTask;
    }

    [Test]
    public Task ShouldBe_Serialise()
    {
        var outboxMessage = new OutboxMessage()
        {
           Metadata = new EventMetadata() {CorrelationID = Guid.NewGuid(),
               EventType = typeof(TempEvent).ToString()
           },
            Message = JsonSerializer.Serialize(new TempEvent()),            
        };

        var capPublisher = new Mock<Assembly>();
        capPublisher.Setup(m => m.GetType(It.IsAny<string>()))
            .Returns(typeof(TempEvent));

        var type = capPublisher.Object.GetType("type");

        var domainEvent = JsonSerializer.Deserialize(outboxMessage.Message, type);
        domainEvent!.GetType().Should().Be(type);
        return Task.CompletedTask;
    }

    [Test]
    public Task EventIsNull_ShouldThrowException()
    {
        var accountContextMock = new Mock<IAccountsDbContext>();
        var capPublisherMock = new Mock<ICapPublisher>();
        var configuration = new Mock<IConfiguration>();
        var capRepo = new CapOutboxRepository(accountContextMock.Object, capPublisherMock.Object, configuration.Object);

        Assert.ThrowsAsync<ArgumentNullException>(async () => await capRepo.SaveAndPublish<Domain.Entities.Member>("sql",(Domain.Entities.Member)null, null));
        return Task.CompletedTask;
    }

    [Test]
    public async Task OutboxHandler_ShouldBeNoException()
    {
        var mediatorMock = new Mock<IMediator>();
        var eventServiceMock = new Mock<IAWSEventServiceFactory>();
        var assemblyMock = new Mock<Assembly>();

        assemblyMock.Setup(m => m.GetType(It.IsAny<string>())).Returns(typeof(TempEvent));

        eventServiceMock.Setup(m => m.GetAWSPublishersForEvent(It.IsAny<DomainEvent>())).Returns(It.IsAny<List<IAWSEventService>>());

        var outboxHandler = new OutboxHandlers(assemblyMock.Object, mediatorMock.Object, eventServiceMock.Object);

        Assert.DoesNotThrowAsync(async () => await outboxHandler.Handle(new OutboxMessage()
        {
            Message = JsonSerializer.Serialize(new TempEvent())
        }));
    }

    [Test]
    public Task OutboxHandler_ShouldBeException()
    {
        var mediatorMock = new Mock<IMediator>();
        var eventServiceMock = new Mock<IAWSEventServiceFactory>();
        var assemblyMock = new Mock<Assembly>();

        assemblyMock.Setup(m => m.GetType(It.IsAny<string>())).Returns(typeof(TempEvent));

        eventServiceMock.Setup(m => m.GetAWSPublishersForEvent(It.IsAny<DomainEvent>())).Throws(new InvalidOperationException());

        var outboxHandler = new OutboxHandlers(assemblyMock.Object, mediatorMock.Object, eventServiceMock.Object);

        Assert.ThrowsAsync<OutboxMessageHandlerNotFoundException>(async () => await outboxHandler.Handle(new OutboxMessage()
        {
            Message = JsonSerializer.Serialize(new TempEvent())
        }));
        return Task.CompletedTask;
    }

    [Test]
    public Task OutboxHandler_EventShouldBeException()
    {
        var mediatorMock = new Mock<IMediator>();
        var eventServiceMock = new Mock<IAWSEventServiceFactory>();
        var assemblyMock = new Mock<Assembly>();

        assemblyMock.Setup(m => m.GetType(It.IsAny<string>())).Returns((Type)null);

        var outboxHandler = new OutboxHandlers(assemblyMock.Object, mediatorMock.Object, eventServiceMock.Object);

        Assert.ThrowsAsync<InvalidOperationException>(async () => await outboxHandler.Handle(new OutboxMessage()
        {
            Message = JsonSerializer.Serialize(new TempEvent())
        }));
        return Task.CompletedTask;
    }
}

public class TempEvent : DomainEvent
{
}