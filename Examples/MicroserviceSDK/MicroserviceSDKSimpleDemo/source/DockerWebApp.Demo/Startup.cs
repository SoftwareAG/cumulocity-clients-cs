using Cumulocity.SDK.Microservices.BasicAuthentication;
using Cumulocity.SDK.Microservices.Configure;
using Cumulocity.SDK.Microservices.Services;
using Cumulocity.SDK.Microservices.Settings;
using Cumulocity.SDK.Microservices.Utils;
using Cumulocity.SDK.Microservices.Utils.Scheduled;
using Cumulocity.SDK.Microservices.Utils.Scheduling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DockerWebApp.Demo
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddCumulocityAuthentication(Configuration);
            services.AddPlatform(Configuration);
            services.AddSingleton<IApplicationService, ApplicationService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Add scheduled tasks & scheduler
            services.AddSingleton<IScheduledTask, CurrentApplicationSubscriptionsTask>();
            services.AddScheduler((sender, args) =>
            {
                Debug.Write(args.Exception.Message);
                args.SetObserved();
            });
            //MVC
            services.AddMvc();
            services.Replace(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(TimedLogger<>)));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseAuthentication();
            app.UseBasicAuthentication();
            app.UseMvcWithDefaultRoute();
        }
    }
}