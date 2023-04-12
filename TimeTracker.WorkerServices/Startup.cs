using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using TimeTracker.Business;
using TimeTracker.Business.Notifications;

namespace TimeTracker.WorkerServices;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddHostedService<Services.Queue.DefaultProcessingHostedService>();
        services.AddHostedService<Services.Queue.NotificationProcessingHostedService>();
        services.AddHostedService<Services.Queue.ExternalClientProcessingHostedService>();
        services.AddHostedService<Services.Tasks.TaskNotificationHostedService>();
        services.AddHostedService<Services.ImageUploadingHostedService>();
    }

    public void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterAssemblyModules(typeof(BusinessAssemblyMarker).Assembly);
        containerBuilder.RegisterAssemblyModules(typeof(BusinessNotificationsAssemblyMarker).Assembly);
    }
    
    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
    }
}
