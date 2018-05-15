
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DemoWebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Xunit;
using System.Text;
using Cumulocity.SDK.Microservices.IntegrationTest.Utils;
using Newtonsoft.Json;

namespace Cumulocity.SDK.Microservices.IntegrationTest.Scenarios
{
	public class TestWebApi
	{
		TestServer _server;
		HttpClient _client;


		public TestWebApi()
		{
			Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
			Environment.SetEnvironmentVariable("C8Y_MICROSERIVCE_ISOLATION", "PER_TENANT");
			Environment.SetEnvironmentVariable("C8Y_BASEURL", "http://management.staging7.c8y.io");
			Environment.SetEnvironmentVariable("C8Y_BASEURL_MQTT", "");
			Environment.SetEnvironmentVariable("C8Y_TENANT", "");
			Environment.SetEnvironmentVariable("C8Y_PASSWORD", "");
			Environment.SetEnvironmentVariable("C8Y_USERNAME", "");
			Environment.SetEnvironmentVariable("SERVER_PORT", "47000");
			Environment.SetEnvironmentVariable("C8Y_BOOTSTRAP_TENANT", "management");
			Environment.SetEnvironmentVariable("C8Y_BOOTSTRAP_USER", "servicebootstrap_cs-combain");
			Environment.SetEnvironmentVariable("C8Y_BOOTSTRAP_PASSWORD", "dpsK5Q5Emm");

			var hostBuilder = new WebHostBuilder()
				.CaptureStartupErrors(true)
				.ConfigureAppConfiguration((builderContext, config) =>
				{
					IHostingEnvironment env = builderContext.HostingEnvironment;
					config
						//.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
						.AddEnvironmentVariables();
				})
				.UseStartup<Startup>();
				

			_server = new TestServer(hostBuilder);

			var credentials = FileUtils.GetCredentials();
			_client = _server.CreateClient();
			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials.Hash);
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		[Fact]
		public async Task asserting_authentication_with_basic_authentication()
		{
			var response = await _client.GetAsync("/api/values");
			response.EnsureSuccessStatusCode();
		}

		[Fact]
		public async Task asserting_api_data_get_subscription()
		{
			var response = await _client.GetAsync("/api/data/subscriptions");
			response.EnsureSuccessStatusCode();
			string responseBody = await response.Content.ReadAsStringAsync();
			dynamic json = JsonConvert.DeserializeObject(responseBody, typeof(object));
			Assert.NotEqual(0, json.Count);
		}
	}	
}
