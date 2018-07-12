using System.Diagnostics;
using Cumulocity.SDK.Microservices.BasicAuthentication;
using Cumulocity.SDK.Microservices.Configure;
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
using Cumulocity.SDK.Microservices.Utils.Scheduling;

namespace DemoWebApi
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
			services.AddPlatform(Configuration);
			ConfigureServicesLayer(services);
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			// Add scheduled tasks & scheduler
			services.AddSingleton<IScheduledTask, TimerTask>();
			services.AddScheduler((sender, args) =>
			{
				Debug.Write(args.Exception.Message);
				args.SetObserved();
			});

			//MVC
			services.AddMvc().AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
			//services.Replace(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(TimedLogger<>)));
		}
		public virtual void ConfigureServicesLayer(IServiceCollection services)
		{
			services.AddCumulocityAuthentication(Configuration);
			services.AddSingleton<IApplicationService, ApplicationService>();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			app.UseAuthentication();
			app.UseBasicAuthentication();
			app.UseMvcWithDefaultRoute();
		}
	}
}
