using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Accounts.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAndConfigureApiVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                options.ReportApiVersions = true;
            });

            services.AddVersionedApiExplorer(options =>
            {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";

                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }

        public static IServiceCollection AddAndConfigureSwagger(this IServiceCollection services)
        {
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerConfigureOptions>();

            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                     {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                     }
                });
            });

            services.ConfigureSwaggerGen(options => { options.CustomSchemaIds(x => x.FullName); });

            return services;
        }

        public static IServiceCollection AddAndConfigureCognitoAuthentication(this IServiceCollection services, IConfiguration Configuration)
        {
            var region = Configuration["AWS:Region"];
            var userPoolId = Configuration["AWS:UserPoolId"];
            var cognitoIssuer = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
            Log.Information("IDP issuer {cognitoIssuer}", cognitoIssuer);
            services.AddAuthentication("Bearer")
                    .AddJwtBearer(options =>
                    {
                        options.Authority = cognitoIssuer;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            ValidateIssuer = true,
                            ValidateLifetime = true,
                            ValidateAudience = false
                        };
                    });

            return services;
        }
    }
}