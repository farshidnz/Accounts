using Accounts.Infrastructure.AWS;
using Amazon;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.UnitTests.AWS
{
    using Accounts.Domain.Enums;
    [TestFixture]
    public class KMSEncryptionServiceTest
    {
       
        // Init Common Test Data 
        private const string KeyArn = "arn:aws:kms:ap-southeast-2:752830773963:key/2ef257a0-1111-1111-1111-230f7f3b5906";
        private const string TestString = "mysecret";

        // Mocks
        private Mock<IConfiguration> configuration;
        private Mock<AmazonKeyManagementServiceClient> kmsClient;
        private Mock<ILogger<KMSEncryptionService>> logger;

        [SetUp]
        public void Setup()
        {
            configuration = new Mock<IConfiguration>();
            kmsClient = new Mock<AmazonKeyManagementServiceClient>(RegionEndpoint.APSoutheast2);
            logger = new Mock<ILogger<KMSEncryptionService>>();

            configuration.SetupGet(p => p[It.Is<string>(s => s == "AccountDomainPIIKeyArn")]).Returns(KeyArn);

            EncryptResponse encryptResponse = new();
            kmsClient.Setup(x => x.EncryptAsync(It.IsAny<EncryptRequest>(), default)).ReturnsAsync(encryptResponse);

            DecryptResponse decryptResponse = new();
            kmsClient.Setup(x => x.DecryptAsync(It.IsAny<DecryptRequest>(), default)).ReturnsAsync(decryptResponse);
        }

        public KMSEncryptionService SUT()
        {
            return new KMSEncryptionService(configuration.Object, logger.Object, kmsClient.Object);
        }

        [Test]
        public async Task WhenAccountDomainPIIKeyArnIsNullOrEmpty_ShouldReturnEmptyString()
        {
            configuration.SetupGet(p => p[It.Is<string>(s => s == "AccountDomainPIIKeyArn")]).Returns("");
            var testString = TestString;

            var encryptString = await SUT().Encrypt(testString);
            encryptString.Should().BeEmpty();

            var encryptStringBase64 = await SUT().Encrypt(testString, EncryptionDataType.Base64);
            encryptStringBase64.Should().BeEmpty();

            var decryptString = await SUT().Decrypt(testString);
            decryptString.Should().BeEmpty();

            var decryptStringBase64 = await SUT().Decrypt(testString,EncryptionDataType.Base64);
            decryptStringBase64.Should().BeEmpty();
        }

        [Test]
        public async Task WhenInvalidKeyArn_ShouldReturnEmptyString()
        {
            configuration.SetupGet(p => p[It.Is<string>(s => s == "AccountDomainPIIKeyArn")]).Returns("test");

            var testString = TestString;

            var encryptString = await SUT().Encrypt(testString);
            encryptString.Should().BeEmpty();

            var encryptStringBase64 = await SUT().Encrypt(testString, EncryptionDataType.Base64);
            encryptStringBase64.Should().BeEmpty();

            var decryptString = await SUT().Decrypt(testString);
            decryptString.Should().BeEmpty();

            var decryptStringBase64 = await SUT().Decrypt(testString, EncryptionDataType.Base64);
            decryptStringBase64.Should().BeEmpty();
        }

        [Test]
        public async Task WhenEncryptAysncRaiseError_ShouldReturnEmptyString()
        {
            var exception = new Exception("error encrypt string");
            kmsClient.Setup(x => x.EncryptAsync(It.IsAny<EncryptRequest>(), default)).Throws(exception);

            var testString = TestString;

            var encryptString = await SUT().Encrypt(testString);
            encryptString.Should().BeEmpty();

            var encryptStringBase64 = await SUT().Encrypt(testString, EncryptionDataType.Base64);
            encryptStringBase64.Should().BeEmpty();
        }

        [Test]
        public async Task WhenEncryptAysnc_ShouldReturnEncryptedString()
        {
            var testString = TestString;

            EncryptResponse encryptResponse = new EncryptResponse();
            var decryptData = Encoding.UTF8.GetBytes(TestString);
            encryptResponse.CiphertextBlob = new System.IO.MemoryStream(decryptData, 0, decryptData.Length);
            kmsClient.Setup(x => x.EncryptAsync(It.IsAny<EncryptRequest>(), default)).ReturnsAsync(encryptResponse);

            var encryptString = await SUT().Encrypt(testString);
            encryptString.Should().NotBeEmpty();

            var encryptStringBase64 = await SUT().Encrypt(testString, EncryptionDataType.Base64);
            encryptStringBase64.Should().NotBeEmpty();
        }

        [Test]
        public async Task WhenDecryptAsyncRaiseError_ShouldReturnEmptyString()
        {
            var exception = new Exception("error decrypt string");
            kmsClient.Setup(x => x.DecryptAsync(It.IsAny<DecryptRequest>(), default)).Throws(exception);

            var testString = TestString;

            var encryptString = await SUT().Decrypt(testString);
            encryptString.Should().BeEmpty();

            var encryptStringBase64 = await SUT().Decrypt(testString, EncryptionDataType.Base64);
            encryptStringBase64.Should().BeEmpty();
        }

        [TestCase(null)]
        [TestCase("")]
        public async Task WhenInputIsNullOrEmpty_ShouldReturnEmptyString(string input)
        {
            var encryptString = await SUT().Decrypt(input);
            encryptString.Should().BeEmpty();

            var encryptStringBase64 = await SUT().Encrypt(input);
            encryptStringBase64.Should().BeEmpty();
        }

        [Test]
        public async Task WhenDecryptAsync_ShouldReturnDecryptedString()
        {
            var testString = TestString;

            DecryptResponse decryptResponse = new();
            var decryptData = Encoding.UTF8.GetBytes(TestString);
            decryptResponse.Plaintext = new System.IO.MemoryStream(decryptData, 0, decryptData.Length);
            kmsClient.Setup(x => x.DecryptAsync(It.IsAny<DecryptRequest>(), default)).ReturnsAsync(decryptResponse);

            var encryptString = await SUT().Decrypt(testString, EncryptionDataType.String);
            encryptString.Should().NotBeEmpty();

            var encryptStringBase64 = await SUT().Decrypt(testString, EncryptionDataType.Base64);
            encryptStringBase64.Should().NotBeEmpty();
        }
    }
}
