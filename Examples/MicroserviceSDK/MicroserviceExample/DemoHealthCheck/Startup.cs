using Cumulocity.SDK.Microservices.BasicAuthentication;
using Cumulocity.SDK.Microservices.Configure;
using Cumulocity.SDK.Microservices.HealthCheck.Extentions;
using Cumulocity.SDK.Microservices.HealthCheck.Extentions.Checks;
using Cumulocity.SDK.Microservices.Services;
using Cumulocity.SDK.Microservices.Settings;
using Cumulocity.SDK.Microservices.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace DemoHealthCheck
{
    public class Startup
    {
	    ILogger _logger;
	    public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
	    {
		    Configuration = configuration;
		    _logger = loggerFactory.CreateLogger<Startup>();
	    }

		public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
	        _logger.LogDebug($"Total Services Initially: {services.Count}");

			services.AddMemoryCache();
            services.AddCumulocityAuthentication(Configuration);
            services.AddPlatform(Configuration);
            services.AddSingleton<IApplicationService, ApplicationService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			//
			services.AddHealthChecks(checks =>
			{
				checks.AddPlatformCheck();
				checks.AddCheck("long-running", async cancellationToken =>
				{
					await Task.Delay(1000, cancellationToken);
					return HealthCheckResult.Healthy("I ran too long");
				});
			});

			//MVC
			services.AddMvc().AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
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
