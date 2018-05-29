using Cumulocity.AspNetCore.Authentication.Basic;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Cumulocity.SDK.Microservices.BasicAuthentication;
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
using Cumulocity.SDK.Microservices.Services;
using Cumulocity.SDK.Microservices.Settings;
using Cumulocity.SDK.Microservices.Utils;
using Moq;

namespace Cumulocity.SDK.Microservices.IntegrationTest
{
	internal static class TestBed
	{
		public static Mock<IApplicationService> ApplicationServiceMock { get; private set; }

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
			ApplicationServiceMock = new Mock<IApplicationService>();

			var builder = new WebHostBuilder()
			   .Configure(app =>
				{
					app.UseAuthentication();
					app.UseBasicAuthentication(); 
				})
				.ConfigureServices((builderContext, services) =>
				{
					services.AddMemoryCache();
					services.AddCumulocityAuthentication(builderContext.Configuration);
					services.AddPlatform(builderContext.Configuration);

					services.Replace(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(TimedLogger<>)));
					builderAction(services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme));
				});

			return new TestServer(builder);
		}
	}
}
