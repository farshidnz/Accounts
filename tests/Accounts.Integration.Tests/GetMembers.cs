using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Accounts.API;
using Accounts.Domain.Entities;
using Accounts.Infrastructure.Persistence;
using Dapper;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using FizzWare.NBuilder;
using FluentAssertions;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Accounts.Integration.Tests;

[Collection(TestCollections.AccountTests)]
public class GetMembers: IAsyncLifetime
{
    private WebHost<Startup>? _webHost;
    private HttpResponseMessage? _result;
    private const string Email = "test@cashrewards.com";

    private readonly TestcontainersContainer _dbContainer =
        new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "Accounts",
                Username = "test",
                Password = "password01",
                Port = 5555
            }).Build();
    
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        _webHost = new WebHost<Startup>();
        var config = _webHost.ServiceProvider.GetRequiredService<IConfiguration>();
        var dbContext = _webHost.ServiceProvider.GetRequiredService<IAccountsDbContext>();
        var migrationRunner = _webHost.ServiceProvider.GetRequiredService<IMigrationRunner>();
        migrationRunner.MigrateUp();

        var connection = dbContext.CreateConnection();

        var member = Builder<Member>
            .CreateNew()
            .With(x => x.Email = Email)
            .With(x => x.Active = true)
            .With(x => x.KycStatusId = 1)
            .Build();
        
        var congintoPool =
            $"INSERT INTO public.cognitopool(cognitopoolid, cognitopoolname, datecreated, active) VALUES (1, 's', '{DateOnly.Parse("2002/12/11")}', true);";
        await connection.ExecuteAsync(congintoPool);

        var sql = $"INSERT INTO public.member(" +
                  //$"id," +
                  $"memberid," +
                  $"cognitoid," +
                  $"premiumstatus, " +
                  $"status," +
                  $"membernewid, " +
                  $"dateofbirth," +
                  $"firstname," +
                  $"lastname, " +
                  $"postcode, " +
                  $"mobile, " +
                  $"email, " +
                  $"ssousername, " +
                  $"ssoprovider, " +
                  $"accesscode, " +
                  $"gender, " +
                  $"receivenewsletter, " +
                  $"smsconsent, " +
                  $"appnotificationconsent, " +
                  $"commspromptshowncount, " +
                  $"clickwindowactive, " +
                  $"popupactive, " +
                  $"isvalidated, " +
                  $"requiredlogin, " +
                  $"isavailable," +
                  $"activeby, " +
                  $"mailchimplistemailid, " +
                  $"datereceivednewsletter, " +
                  $"communicationemail, " +
                  $"paypalemail, " +
                  $"campaignid, " +
                  $"twofactorauthid, " +
                  $"istwofactorauthenticatedenable, " +
                  $"twofactorauthenticatedactivatedby, " +
                  $"twofactorauthenticatedmobile, " +
                  $"twofactorauthenticatedcountrycode, " +
                  $"riskdescription, " +
                  $"isrisky, " +
                  $"lastlogon, " +
                  $"datedeletedbymember, " +
                  $"datejoined, " +
                  $"datereceivenewsletter, " +
                  $"iswithdrawalcapped, " +
                  $"kycstatusid, " +
                  $"signupverificationemailsentstatus, " +
                  $"cognitopoolid, " +
                  $"originationsource, " +
                  $"active) " +
                  $" VALUES ('{member.MemberId}'," +
                  $"'{member.CognitoId}'," +
                  $"{member.PremiumStatus}," +
                  $"{member.Status}," +
                  $"'{member.MemberNewId}'," +
                  $"'{DateOnly.Parse("2002/12/11")}'," +
                  $"'{member.FirstName}'," +
                  $"'{member.LastName}'," +
                  $"'5555'," +
                  $"'{member.Mobile}'," +
                  $"'{member.Email}'," +
                  $"'{member.SSOUsername}'," +
                  $"'{member.SSOProvider}'," +
                  $"'x'," +
                  $"'{1}'," +
                  $"{member.ReceiveNewsLetter}," +
                  $"{member.SmsConsent}," +
                  $"{member.AppNotificationConsent}," +
                  $"{member.CommsPromptShownCount}," +
                  $"{member.ClickWindowActive}," +
                  $"{member.PopUpActive}," +
                  $"{member.IsValidated}," +
                  $"{member.RequiredLogin}," +
                  $"{member.IsAvailable}," +
                  $"'{DateOnly.Parse("2022/02/11")}'," +
                  $"'{member.MailChimpListEmailId}'," +
                  $"'{DateOnly.Parse("2022/02/11")}'," +
                  $"'{member.CommunicationEmail}'," +
                  $"'{member.PayPalEmail}'," +
                  $"{member.CampaignId}," +
                  $"'{member.TwoFactorAuthId}'," +
                  $"{member.TwoFactorAuthenticatedEnable}," +
                  $"'2020/12/11'," +
                  $"'{"sd"}'," +
                  $"'102'," +
                  $"'{member.RiskDescription}'," +
                  $"{member.IsRisky}," +
                  $"'2020/12/11'," +
                  $"'2020/12/11'," +
                  $"'{DateOnly.Parse("2020/12/11")}'," +
                  $"'{DateOnly.Parse("2022/02/11")}'," +
                  $"{member.IsWithdrawalCapped}," +
                  $"{member.KycStatusId}," +
                  $"{member.SignUpVerificationEmailSentStatus},"+
                  $"{member.CognitoPoolId},"+
                  $"'{member.OriginationSource}',"+
                  $"{member.Active})";
        await connection.ExecuteAsync(sql);
        
        _result = await _webHost.Client.GetAsync($"v1/Member/{Email}");
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        _result!.Dispose();
        _webHost!.Dispose();
    }
    
    [Fact(Skip = "until we decide to use it")]
    public void ShouldBe_CorrectResponse()
    {
        _result!.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}