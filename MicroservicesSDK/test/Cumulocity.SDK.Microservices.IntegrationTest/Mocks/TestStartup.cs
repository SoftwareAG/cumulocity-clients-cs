using DemoWebApi;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cumulocity.SDK.Microservices.IntegrationTest.Mocks
{
	public class TestStartup : Startup
	{
		public TestStartup(IConfiguration configuration, IHostingEnvironment env, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
		{
			Configuration = configuration;
			Environment = env;
		}

		public IConfiguration Configuration { get; }

		public IHostingEnvironment Environment { get; }
		public void ConfigureTestServices(IServiceCollection services)
		{
			base.ConfigureServices(services);
			//services.Replace<IService, IMockedService>();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			base.Configure(app, env, loggerFactory);
		}
	}
}
