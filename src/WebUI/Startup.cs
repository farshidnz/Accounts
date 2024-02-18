using Accounts.API.Extensions;
using Accounts.API.Middlewares;
using Accounts.API.Security;
using Accounts.API.Services;
using Accounts.Application;
using Accounts.Application.Common.Interfaces;
using Accounts.Application.Member.Commands.UpdateMember.v1;
using Accounts.Infrastructure;
using Accounts.Infrastructure.AWS;
using Accounts.Infrastructure.Configuration;
using Accounts.Infrastructure.Persistence;
using Accounts.Infrastructure.Persistence.Handlers;
using Accounts.Infrastructure.Services;
using Amazon.SimpleSystemsManagement.Model;
using Dapper;
using FluentMigrator.Runner;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Threading.Tasks;

namespace Accounts.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _dbConfiguration = new DbConfiguration();
        }

        public IConfiguration Configuration { get; }
        private DbConfiguration _dbConfiguration { get; set; }

        public string GetConnectionStr(IServiceCollection services)
        {
            return GetConnectionString(services).Result;
        }

        public async Task<string> GetConnectionString(IServiceCollection services)
        {
            var EnableDevops4 = bool.TryParse(Configuration["EnableDevops4"], out var b) && b;
            var sp = services.BuildServiceProvider();
            if (EnableDevops4)
            {
                var awsSecretsManagerClientFactory = sp.GetRequiredService<IAwsSecretsManagerClientFactory>();
                if (awsSecretsManagerClientFactory == null)
                    throw new InvalidOperationException();

                var AccountSecret = await awsSecretsManagerClientFactory.GetSecretValueAsync<AWSAccountsSecret>(Configuration["DbSecretName"]);
                if (AccountSecret == null)
                    throw new InvalidResourceIdException("Accounts Database Credential doesn't exist!");

                _dbConfiguration.host = AccountSecret.host;
                _dbConfiguration.port = AccountSecret.port.ToString();
                _dbConfiguration.dbname = AccountSecret.dbname;
                _dbConfiguration.username = AccountSecret.username;
                _dbConfiguration.password = AccountSecret.password;
            }
            else
            {  // for local debug and integration test in pipeline
                var AccoundDbString = Configuration.GetConnectionString("AccountsDB");

                NpgsqlConnectionStringBuilder dbConnectionStringBuilder = new NpgsqlConnectionStringBuilder(AccoundDbString);

                _dbConfiguration.host = (string)dbConnectionStringBuilder["Host"];
                _dbConfiguration.port = dbConnectionStringBuilder["Port"].ToString();
                _dbConfiguration.dbname = (string)dbConnectionStringBuilder["Database"];
                _dbConfiguration.username = (string)dbConnectionStringBuilder["Username"];
                _dbConfiguration.password = (string)dbConnectionStringBuilder["Password"];
            }

            // It should use the application user credential
            string connectionString = $"Server={_dbConfiguration.host};Port={_dbConfiguration.port};Database={_dbConfiguration.dbname};User Id={Configuration["PostgresDbUsername"].ToLower()};Password={Configuration["PostgresDbPassword"]}";

            return connectionString;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services.AddControllers();

            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());

            services.AddSingleton<ICurrentUserService, CurrentUserService>();

            services.AddHttpContextAccessor();

            // Customise default API behaviour
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddAndConfigureApiVersioning();

            services.AddAndConfigureSwagger();

            services.AddAndConfigureCognitoAuthentication(Configuration);
            services.AddValidatorsFromAssemblyContaining<UpdateMemberCommandValidator>();

            services.AddApplication();
            services.AddHealthChecks();

            services.AddTransient<IAwsSecretsManagerClientFactory, AwsSecretsManagerClientFactory>();
            services.AddTransient<ISimpleSystemManagementService, AwsSimpleSystemManagementService>();
            var connectionString = GetConnectionStr(services);
            services.AddSingleton<DbConfiguration>(_dbConfiguration);
            services.AddTransient<IAccountsDbContext, AccountsDbContext>();
            services.AddSingleton<IDatabaseConfigurator, DatabaseConfigurator>();
            services.AddInfrastructure(Configuration);
            SqlMapper.AddTypeHandler(new GuidTypeHandler());

            services.AddFluentMigratorCore()
                .ConfigureRunner(c => c.AddPostgres().WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(Infrastructure.DependencyInjection).Assembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole().SetMinimumLevel(LogLevel.Error));

            services.AddTransient<IApplicationKeyValidation, ApplicationKeyValidationService>();
            services.AddOptions<AWSAccountsSecret>();
            services.AddOptions<AWSOptions>();
            services.AddSingleton<IAuthorizationHandler, ApplicationKeyValidationHandler>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("InternalPolicy",
                    policy => policy.Requirements.Add(new ApplicationKeyValidationRequirement()));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UsePathBase("/api/accounts");

            app.UseSwagger(c =>
            {
                c.RouteTemplate = "swagger/{documentname}/swagger.json";
            });
            app.UseSwaggerUI(c =>
            {
                foreach (var desc in provider.ApiVersionDescriptions)
                {
                    c.RoutePrefix = "swagger";
                    c.SwaggerEndpoint($"{desc.GroupName}/swagger.json", $"API v{desc.ApiVersion}");
                }
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health-check");
            });
        }
    }
}