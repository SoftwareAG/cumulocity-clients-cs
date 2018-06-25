using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Cumulocity.AspNetCore.Authentication.Basic;
using Cumulocity.SDK.Microservices.FunctionalTest.Mocks;
using Cumulocity.SDK.Microservices.FunctionalTest.Utils;
using Cumulocity.SDK.Microservices.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Cumulocity.SDK.Microservices.FunctionalTest
{
	public class TestFixture<TStartup> : IDisposable where TStartup : class
	{
		public readonly TestServer Server;
		public readonly Mock<IApplicationService> ApplicationServiceMock;
		public readonly ApplicationServicClient AppServiceClient;

		public TestFixture()
		{
		    ApplicationServiceMock = new Mock<IApplicationService>();

			var builder = new WebHostBuilder()
				.UseStartup<TStartup>()
				.ConfigureServices(services =>
				{
					services.AddMemoryCache();
					services.AddSingleton(ApplicationServiceMock.Object);				
				});


			Server = new TestServer(builder);
			var httpClient = Server.CreateClient();
			AppServiceClient = new ApplicationServicClient(httpClient);

		}


		public void Dispose()
		{
			Server.Dispose();
		}
	}


	public class TestFixtureLive<TStartup> : IDisposable where TStartup : class
	{
		public readonly TestServer Server;

		public readonly ApplicationServicClient AppServiceClient;

		public TestFixtureLive()
		{
			var creds = FileUtils.GetCredentials();
			var builder = new WebHostBuilder()
				.ConfigureAppConfiguration((builderContext, config) =>
				{
					IHostingEnvironment env = builderContext.HostingEnvironment;
					config
						//.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
						.AddEnvironmentVariables();
				})
				.UseStartup<TStartup>()
				.ConfigureServices(services =>
				{
					services.AddMemoryCache();

				});

			Server = new TestServer(builder);
			var httpClient = Server.CreateClient();
			AppServiceClient = new ApplicationServicClient(httpClient, creds);

		}


		public void Dispose()
		{
			Server.Dispose();
		}
	}
}
