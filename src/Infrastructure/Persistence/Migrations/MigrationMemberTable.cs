using FluentMigrator;
using System;

namespace Accounts.Infrastructure.Persistence.Migrations
{
    [Migration(1)]
    public class MigrationMemberTable : Migration
    {
        public override void Up()
        {
            Create.Table("memberclientmap")
              .WithColumn("memberclientmapid").AsInt32().NotNullable().PrimaryKey().Identity()
              .WithColumn("clientid").AsInt32().NotNullable()
              .WithColumn("memberid").AsInt32().NotNullable()
              .WithColumn("isprimaryclient").AsBoolean().NotNullable().WithDefaultValue(false)
              .WithColumn("datecreated").AsDateTime2().NotNullable().WithDefaultValue(DateTime.UtcNow)
              .WithColumn("active").AsBoolean().NotNullable().WithDefaultValue(true);

            Create.ForeignKey("fk_memberclientmap_clientid_client_clientid")
            .FromTable("memberclientmap").ForeignColumn("clientid")
            .ToTable("client").PrimaryColumn("clientid");

            Create.Table("member")
            .WithColumn("id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("memberid").AsInt32().NotNullable().Unique().Identity()
            .WithColumn("cognitoid").AsGuid().Nullable()
            .WithColumn("premiumstatus").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("status").AsInt16().NotNullable()
            .WithColumn("membernewid").AsString(100).Nullable()
            .WithColumn("dateofbirth").AsDateTime2().Nullable()
            .WithColumn("firstname").AsString(100).Nullable()
            .WithColumn("lastname").AsString(100).Nullable()
            .WithColumn("postcode").AsString(10).Nullable()
            .WithColumn("mobile").AsString(15).Nullable()
            .WithColumn("email").AsString(100).Nullable()

            .WithColumn("ssousername").AsString(100).Nullable()
            .WithColumn("ssoprovider").AsString(100).Nullable()
            .WithColumn("accesscode").AsString(200).Nullable()
            .WithColumn("gender").AsInt16().Nullable()
            .WithColumn("receivenewsletter").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("smsconsent").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("appnotificationconsent").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("commspromptshowncount").AsInt16().NotNullable().WithDefaultValue(0)
            .WithColumn("clickwindowactive").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("popupactive").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("isvalidated").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("requiredlogin").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("isavailable").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("activeby").AsDateTime2().Nullable()
            .WithColumn("mailchimplistemailid").AsString(50).Nullable()
            .WithColumn("datereceivednewsletter").AsDateTime2().Nullable()
            .WithColumn("communicationemail").AsString(200).Nullable()
            .WithColumn("paypalemail").AsString(200).Nullable()

            .WithColumn("campaignid").AsInt64().Nullable()
            .WithColumn("installnotifier").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("twofactorauthid").AsString(50).Nullable()
            .WithColumn("istwofactorauthenticatedenable").AsBoolean().Nullable()
            .WithColumn("twofactorauthenticatedactivatedby").AsDateTime2().Nullable()
            .WithColumn("twofactorauthenticatedmobile").AsString(50).Nullable()
            .WithColumn("twofactorauthenticatedcountrycode").AsString(10).Nullable()
            .WithColumn("riskdescription").AsString(200).Nullable()
            .WithColumn("isrisky").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("lastlogon").AsDateTime2().Nullable()
            .WithColumn("datedeletedbymember").AsDateTime2().Nullable()
            .WithColumn("datejoined").AsDateTime2().Nullable()
            .WithColumn("datereceivenewsletter").AsDateTime2().Nullable()
            .WithColumn("comment").AsString().Nullable()
            .WithColumn("iswithdrawalcapped").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("kycstatusid").AsInt16().NotNullable()
            .WithColumn("signupverificationemailsentstatus").AsInt16().NotNullable().WithDefaultValue(0)
            .WithColumn("cognitopoolid").AsInt16().NotNullable().WithDefaultValue(0)
            .WithColumn("trustscore").AsInt16().NotNullable().WithDefaultValue(0)
            .WithColumn("originationsource").AsString(255).Nullable()
            .WithColumn("active").AsBoolean().NotNullable().WithDefaultValue(true);

            Create.ForeignKey("fk_member_kycstatusid_kycstatus_kycstatusid")
             .FromTable("member").ForeignColumn("kycstatusid")
             .ToTable("kycstatus").PrimaryColumn("kycstatusid");

            Create.ForeignKey("fk_member_cognitopoolid_cognitopool_cognitopoolid")
            .FromTable("member").ForeignColumn("cognitopoolid")
            .ToTable("cognitopool").PrimaryColumn("cognitopoolid");

            Create.ForeignKey("fk_memberclientmap_memberid_member_memberid")
              .FromTable("memberclientmap").ForeignColumn("memberid")
              .ToTable("member").PrimaryColumn("memberid");

            Create.Index("idx_member_email")
                .OnTable("member")
                .OnColumn("email")
                .Unique()
                .WithOptions()
                .NonClustered();

            Create.Index("idx_member_memberid")
                .OnTable("member")
                .OnColumn("memberid")
                .Ascending()
                .WithOptions()
                .NonClustered();
        }

        public override void Down()
        {
            Delete.ForeignKey("fk_memberclientmap_clientid_client_clientid").OnTable("memberclientmap");
            Delete.ForeignKey("fk_member_kycstatusid_kycstatus_kycstatusid").OnTable("member");
            Delete.ForeignKey("fk_member_cognitopoolid_cognitopool_cognitopoolid").OnTable("member");
            Delete.ForeignKey("fk_memberclientmap_memberid_member_memberid").OnTable("memberclientmap");
            Delete.Index("idx_member_email").OnTable("member");
            Delete.Index("idx_member_memberid").OnTable("member");
            Delete.Table("memberclientmap");
            Delete.Table("member");
        }
    }
}