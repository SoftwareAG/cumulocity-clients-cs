using System;
using Cumulocity.SDK.Microservices.IntegrationTest.Client;
using Cumulocity.SDK.Microservices.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;


namespace Cumulocity.SDK.Microservices.IntegrationTest
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
}
