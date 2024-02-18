using Accounts.API.Security;
using Accounts.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Infrastructure.UnitTests.Services
{
    [TestFixture]
    public class ApplicationKeyValidationHandlerTest
    {
        private Mock<IHttpContextAccessor> _httpContextAccessor;
        private Mock<IApplicationKeyValidation> _ApplicationKeyValidationService;

        [SetUp]
        public void Setup()
        {
            this._httpContextAccessor = new Mock<IHttpContextAccessor>();

            this._ApplicationKeyValidationService = new Mock<IApplicationKeyValidation>();
        }

        private ApplicationKeyValidationHandler SUT()
        {
            return new ApplicationKeyValidationHandler(_httpContextAccessor.Object, _ApplicationKeyValidationService.Object);
        }

        [Test]
        public async Task HandleValidation_ShouldBe_True()
        {
            var author = "author";
            var user = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new Claim[] {
                        new Claim(ClaimsIdentity.DefaultNameClaimType, author),
                            },
                            "Basic")
                        );
            var requirements = new ApplicationKeyValidationRequirement();

            var context = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirements }, user, null);
            await SUT().HandleAsync(context);

            context.HasSucceeded.Should().BeTrue();
        }
    }
}