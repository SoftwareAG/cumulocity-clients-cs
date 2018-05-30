using Cumulocity.AspNetCore.Authentication.Basic;
using Cumulocity.SDK.Microservices.Model;
using Cumulocity.SDK.Microservices.Services;
using Cumulocity.SDK.Microservices.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Cumulocity.SDK.Microservices.BasicAuthentication
{
	public class BasicCredentialVerifier : IBasicCredentialVerifier
	{
		private readonly Platform _platform;
		private readonly IMemoryCache _cache;
		private readonly IApplicationService _service;
		private readonly HttpContext _context;
		protected ILogger Logger { get; }
		public BasicCredentialVerifier(ILoggerFactory logger, Platform platform, IMemoryCache cache, IApplicationService service, IHttpContextAccessor httpContextAccessor)
		{
			Logger = logger.CreateLogger<BasicCredentialVerifier>();
			this._platform = platform;
			this._cache = cache;
			this._service = service;
			this._context = httpContextAccessor.HttpContext;
		}

		public Task<BasicAuthenticationResult> Authenticate(string username, string password)
		{
			return GetCurrentUserAsync(username, password);
		}

		private async Task<BasicAuthenticationResult> GetCurrentUserAsync(string username, string password)
		{
			var authCred = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(username + ":" + password));
			BasicAuthenticationResult cacheEntry2 = await GetBasicAuthenticationResultAsync(username, authCred);

			return cacheEntry2;
		}

		private async Task<BasicAuthenticationResult> GetBasicAuthenticationResultAsync(string username, string authCred)
		{
			Logger.LogInformation("GetBasicAuthenticationResult username: {username} {authCred}.", username, authCred);

			Subscription requiredSubscription;
			var currentUser = await _cache.GetOrCreateAsync(username + "_claims", entry =>
			{
				entry.SlidingExpiration = TimeSpan.FromSeconds(60);
				return _service.GetCurrentUser(authCred);
			});

			var cacheSubscriptions = await
				_cache.GetOrCreateAsync("current_app_subscriptions", entry => _service.GetCurrentApplicationSubscriptions());

			if (!String.IsNullOrEmpty(username.Split("/")[0]))
			{
				requiredSubscription = cacheSubscriptions.FirstOrDefault(s => s.Tenant.Equals(username.Split("/")[0]));

				if (requiredSubscription?.Name != null && requiredSubscription.Password != null)
				{
					currentUser.User = requiredSubscription.Name;
					currentUser.Password = requiredSubscription.Password;
					currentUser.Tenant = requiredSubscription.Tenant;
				}
			}

			return currentUser;
		}
	}
}