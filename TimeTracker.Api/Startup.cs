using Autofac;
using Microsoft.AspNetCore.HttpOverrides;
using Persistence.Transactions.Behaviors;
using Serilog;
using TimeTracker.Api.Di.Autofac.Modules;
using TimeTracker.Api.Middleware;
using TimeTracker.Api.WebSocket.Hubs;
using TimeTracker.Business;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Helpers;
using TimeTracker.Business.Mvc.Middleware;

namespace TimeTracker.Api;

public class Startup
{
    private readonly bool _isRequestResponseLoggingEnabled;

    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        _isRequestResponseLoggingEnabled = configuration.GetValue("App:EnableRequestResponseLogging", false);
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public virtual void ConfigureServices(IServiceCollection services)
    {
        var assembly = typeof(ApiAssemblyMarker).Assembly;
        services.AddCors(options =>
        {
            options.AddPolicy("Cors", policy =>
            {
                if (ApplicationHelper.HostingEnvironment == "Development")
                {
                    policy.WithOrigins(
                        // API
                        "https://dev.timevic.com",
                        "https://dev-api.timevic.com",
                        "https://localhost:7108",
                        "http://localhost:5265",
                        
                        // Web
                        "https://localhost:7230",
                        "http://localhost:5254",
                        "http://localhost:5000"
                    );    
                }
                else
                {
                    policy.WithOrigins(
                        "https://timevic.com",
                        "https://api.timevic.com"
                    );   
                }

                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
        services.AddAutoMapper(cfg => {}, assembly);
        
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });
        services.InitControllers(assembly);
        services.InitApiAuthServices(Configuration);
        
        // Disable X-Frame headers
        services.AddAntiforgery(o => o.SuppressXFrameOptionsHeader = true);
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        });
    }

    public virtual void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        containerBuilder
            .RegisterModule<ApiModule>()
            .RegisterAssemblyModules(typeof(BusinessAssemblyMarker).Assembly);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Build NHibernate metadata during startup so it is not charged to the first request.
        app.ApplicationServices
            .GetRequiredService<IDbConnectionFactory>()
            .GetSessionFactoryAsync()
            .GetAwaiter()
            .GetResult();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSerilogRequestLogging();
        }

        if (_isRequestResponseLoggingEnabled)
        {
            app.UseMiddleware<RequestResponseLoggerMiddleware>();
        }

        app.UseForwardedHeaders();
        app.UseRouting();
        app.UseCors("Cors");
        
        app.UseMiddleware<CommitPerformerMiddleware>();
        app.UseMiddleware<JwtRefreshMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            
            endpoints.MapHub<MessagingHub>("/websocket/messaging");
            endpoints.MapHub<PingHub>("/websocket/ping");
        });
    }
}
