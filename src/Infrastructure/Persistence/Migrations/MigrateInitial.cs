using FluentMigrator;
using System;
namespace Accounts.Infrastructure.Persistence.Migrations;

[Migration(0)]
public class MigrateInitial : Migration
{
    public override void Up()
    {
        Create.Table("client")
        .WithColumn("clientid").AsInt32().NotNullable().PrimaryKey()
        .WithColumn("name").AsString(255).NotNullable()
        .WithColumn("datecreated").AsDateTime2().NotNullable().WithDefaultValue(DateTime.UtcNow)
        .WithColumn("active").AsBoolean().NotNullable().WithDefaultValue(true);

        Insert.IntoTable("client").Row(new { clientid = 1000000, name = "Cashrewards" });
        Insert.IntoTable("client").Row(new { clientid = 1000034, name = "ANZ" });
        Insert.IntoTable("client").Row(new { clientid = 1000033, name = "MoneyMe" });

        Create.Table("kycstatus")
       .WithColumn("kycstatusid").AsInt32().NotNullable().PrimaryKey().Identity()
       .WithColumn("description").AsString(255).NotNullable()
       .WithColumn("datecreated").AsDateTime2().NotNullable().WithDefaultValue(DateTime.UtcNow)
       .WithColumn("active").AsBoolean().NotNullable().WithDefaultValue(true);

        Insert.IntoTable("kycstatus").Row(new { kycstatusid = 1, description = "No" });
        Insert.IntoTable("kycstatus").Row(new { kycstatusid = 2, description = "Emailed" });
        Insert.IntoTable("kycstatus").Row(new { kycstatusid = 3, description = "Documents Received" });
        Insert.IntoTable("kycstatus").Row(new { kycstatusid = 4, description = "Refused To Comply" });
        Insert.IntoTable("kycstatus").Row(new { kycstatusid = 5, description = "Yes" });

        Create.Table("cognitopool")
      .WithColumn("cognitopoolid").AsInt32().NotNullable().PrimaryKey().Identity()
      .WithColumn("cognitopoolname").AsString(255).NotNullable()
      .WithColumn("datecreated").AsDateTime2().NotNullable().WithDefaultValue(DateTime.UtcNow)
      .WithColumn("active").AsBoolean().NotNullable().WithDefaultValue(true);
    }

    public override void Down()
    {
        Delete.Table("client");
        Delete.Table("kycstatus");
        Delete.Table("cognitopool");
    }
}