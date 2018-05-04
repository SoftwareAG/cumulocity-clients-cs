using System;
using System.Collections.Generic;
using System.Text;
using Cumulocity.SDK.Microservices.IntegrationTest.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Cumulocity.SDK.Microservices.IntegrationTest
{
	public abstract class BaseTest
	{

		public BaseTest()
		{

			var server = new TestServer(new WebHostBuilder()
				.UseStartup<StartupMock>()
				.ConfigureServices(services =>
				{
					
				}));

			var httpClient = server.CreateClient();
		}

		//[TestCleanup]
		public void BaseTearDown()
		{
		}
	}
}
