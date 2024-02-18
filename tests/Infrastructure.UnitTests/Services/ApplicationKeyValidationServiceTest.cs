using Accounts.Domain.Common;
using Accounts.Infrastructure.AWS;
using Accounts.Infrastructure.Services;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Infrastructure.UnitTests.Services;

[TestFixture]
public class ApplicationKeyValidationServiceTest
{
    private Mock<ISimpleSystemManagementService> _managementService;

    [SetUp]
    public void Setup()
    {
        this._managementService = new Mock<ISimpleSystemManagementService>();

        _managementService.Setup(config => config.GetParameter(It.IsAny<string>())).Returns(new CrApplication("12345,1323,1233"));
    }

    public ApplicationKeyValidationService SUT()
    {
        return new ApplicationKeyValidationService(_managementService.Object);
    }

    [Test]
    public void TestApplicationKeyValidationService_Should_BeTrue()
    {
        SUT().IsValid("12345").Should().BeTrue();
    }
    
    [Test]
    public void TestApplicationKeyValidationService_Should_BeFalse()
    {
        SUT().IsValid("989").Should().BeFalse();
    }
}