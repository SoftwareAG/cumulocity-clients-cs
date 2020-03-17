using Cumulocity.SDK.Microservices.BasicAuthentication;
using Cumulocity.SDK.Microservices.HealthCheck.AspNetCore;
using Cumulocity.SDK.Microservices.HealthCheck.Extentions;
using Cumulocity.SDK.Microservices.HealthCheck.Extentions.Checks;
using Cumulocity.SDK.Microservices.Services;
using Cumulocity.SDK.Microservices.Settings;
using Cumulocity.SDK.Microservices.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Net;

namespace Cumulocity.SDK.Microservices.Configure
{
	public static class MicroserviceApplicationExtensions
	{
		public static IWebHostBuilder UseMicroserviceApplication(this IWebHostBuilder builder, bool isHealthCheck=true)
		{
		    if (isHealthCheck)
		    {
		        builder.UseHealthChecks("/health", TimeSpan.FromSeconds(5));
		    }

		    builder.ConfigureServices((builderContext, services) =>
			{
				services.AddMemoryCache();
				services.AddCumulocityAuthentication(builderContext.Configuration);
				services.AddPlatform(builderContext.Configuration);

				services.AddSingleton<IApplicationService, ApplicationService>();
				services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			    if (isHealthCheck)
			    {
			        services.AddHealthChecks(checks => { checks.AddPlatformCheck(); });
			    }

			    services.Replace(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(TimedLogger<>)));
				services.AddSingleton<IStartupFilter, ConfigureSecurityFilter>();
			});

			return builder;
		}
	}

	public class ConfigureSecurityFilter : IStartupFilter
	{
		public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
		{
			return app =>
			{
				app.UseAuthentication();
				app.UseBasicAuthentication();
				next(app);
			};
		}
	}
}