using Cumulocity.AspNetCore.Authentication.Basic;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Cumulocity.SDK.Microservices.Configure;
using Cumulocity.SDK.Microservices.HealthCheck.Extentions;
using Cumulocity.SDK.Microservices.HealthCheck.Extentions.Checks;
using Cumulocity.SDK.Microservices.Services;
using Cumulocity.SDK.Microservices.Settings;
using Cumulocity.SDK.Microservices.Utils;

namespace Cumulocity.SDK.Microservices.IntegrationTest
{
	internal static class TestBed
	{
		public static void SetBasic(this HttpClient client, string username, string password)
		{
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
				Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));
		}

		public static HttpClient GetClient(Action<AuthenticationBuilder> builderAction)
		{
			return CreateServer(builderAction).CreateClient();
		}

		public static TestServer CreateServer(Action<AuthenticationBuilder> builderAction)
		{
			var builder = new WebHostBuilder()
			   .Configure(app =>
				{
					app.UseAuthentication();
					app.Use(async (context, next) =>
					{
						if (context.Request.Path == new PathString("/"))
						{
							var result = await context.AuthenticateAsync(BasicAuthenticationDefaults.AuthenticationScheme);
							if (!result.Succeeded)
								await context.ChallengeAsync(BasicAuthenticationDefaults.AuthenticationScheme);
						}
						else
						{
							await next();
						}
					});
				})
				.ConfigureServices((builderContext, services) =>
				{
					services.AddMemoryCache();
					services.AddCumulocityAuthentication(builderContext.Configuration);
					services.AddPlatform(builderContext.Configuration);
					services.AddSingleton<IApplicationService, ApplicationService>();
					services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

					services.AddHealthChecks(checks =>
					{
						//1
						checks.AddPlatformCheck();
		
					});
					services.Replace(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(TimedLogger<>)));
					builderAction(services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme));
				});

			return new TestServer(builder);
		}
	}
}
