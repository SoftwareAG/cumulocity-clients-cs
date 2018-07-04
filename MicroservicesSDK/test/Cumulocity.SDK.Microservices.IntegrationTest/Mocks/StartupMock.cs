using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cumulocity.AspNetCore.Authentication.Basic;
using Cumulocity.SDK.Microservices.Services;
using DemoWebApi;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cumulocity.SDK.Microservices.IntegrationTest.Mocks
{
	public class StartupMock : Startup
	{
		private readonly IApplicationService _applicationService;
		private IMemoryCache _cache;
		private IConfiguration _configuration;

		public StartupMock(IApplicationService applicationService, IMemoryCache cache, IConfiguration configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
		{
			_configuration = configuration;
			_applicationService = applicationService;
			_cache = cache;
		}

		public override void ConfigureServicesLayer(IServiceCollection services)
		{
			services.AddAuthentication().AddBasicAuthentication<PositiveBasicCredentialVerifier>();
			services.AddSingleton(_applicationService);
		}

	}
	internal class PositiveBasicCredentialVerifier : IBasicCredentialVerifier
	{
		private readonly IMemoryCache _cache;
		private readonly IApplicationService _applicationService;
		public PositiveBasicCredentialVerifier(IMemoryCache cache, IApplicationService applicationService)
		{
			_cache = cache;
			_applicationService = applicationService;
		}
		public async Task<BasicAuthenticationResult> Authenticate(string username, string password)
		{
			var cacheSubscriptions = await
				_cache.GetOrCreateAsync("current_app_subscriptions",
					entry => _applicationService.GetCurrentApplicationSubscriptions());

			return await Task.FromResult(new BasicAuthenticationResult() { IsAuthenticated = true });
		}
	}
}
