using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using TimeTracker.Api;
using TimeTracker.Business.Extensions;

namespace OffLogs.Api.Extensions
{
    public static class AppInitExtensions
    {
        public static void InitControllers(this IServiceCollection services)
        {
            services.AddControllers()
                // We should provide correct assembly for the tests
                .AddApplicationPart(typeof(ApiAssemblyMarker).Assembly)
                .ConfigureApiBehaviorOptions(options =>
                {
                    // Disable pre-model validation of the models
                    options.SuppressModelStateInvalidFilter = true;
                    
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        // Get an instance of ILogger (see below) and log accordingly.
                        var body = context.HttpContext.Request.ReadBodyAsync().Result;
                        Log.Logger.Error($"Request data: {body}");
                        foreach (var value in context.ModelState.Values)
                        {
                            foreach (var error in value.Errors)
                            {
                                var errorMessage = !string.IsNullOrEmpty(error.ErrorMessage)
                                    ? error.ErrorMessage
                                    : error.Exception?.Message;
                                Log.Logger.Error(errorMessage);
                            }
                        }
                        return new BadRequestObjectResult(context.ModelState);
                    };
                })
                .AddNewtonsoftJson(options =>
                {
                    // Remove nullable fields from response Json
                    // options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    
                    // This is fix for the Headers. This resolver fix
                    // a bug when "authorization" header is not equals "Authorization" 
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                })
                .AddJsonOptions(options => {
                    // Ignore Null values in response models
                    // options.JsonSerializerOptions.IgnoreNullValues = true;
                });
        }

        public static void InitAuthServices(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSecurityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    configuration.GetValue<string>("App:Auth:SymmetricSecurityKey")
                )
            );
            services.AddAuthentication(options =>
                {
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = configuration.GetValue<string>("App:Auth:Issuer"),
                        ValidAudience = configuration.GetValue<string>("App:Auth:Audience"), 
                        IssuerSigningKey = jwtSecurityKey,
                        ValidateLifetime = true,
                        ClockSkew = System.TimeSpan.FromMinutes(30000)
                    };
                });
        }
    }
}
