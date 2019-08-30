using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cumulocity.SDK.Microservices.OAuth.Handler;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Cumulocity.SDK.Microservices.Model;
using Cumulocity.SDK.Microservices.OAuth.Handler;
using Cumulocity.SDK.Microservices.Services;
using Cumulocity.SDK.Microservices.Services.OAuth;
using Cumulocity.SDK.Microservices.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Cumulocity.SDK.Microservices.OAuth
{
    public class OAuthVerifier: IOAuthCredentialVerifier
    {
        private readonly Platform _platform;
        private readonly IMemoryCache _cache;
        private readonly IOAuthApplicationService _service;
        private readonly HttpContext _context;

        public OAuthVerifier(Platform platform, IMemoryCache cache, IOAuthApplicationService service, IHttpContextAccessor httpContextAccessor)
        {
            this._platform = platform;
            this._cache = cache;
            this._service = service;
            this._context = httpContextAccessor.HttpContext;
        }

        public async Task<OAuthAuthenticationResult> AuthenticateAsync(OAuthAuthenticationOptions options,
            IHeaderDictionary headers)
        {
            var user = await _service.GetCurrentUser(headers);

            var currentUser = await
                _cache.GetOrCreateAsync($"{user.User}_claims", entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromSeconds(600);
                    return _service.GetCurrentUser(headers);
                });

            var cacheSubscriptions = await
                _cache.GetOrCreateAsync("current_app_subscriptions", entry =>
                    {
                        return _service.GetCurrentApplicationSubscriptionsAsync();
                    });

            if (!String.IsNullOrEmpty(user.User))
            {
                Subscription requiredSubscription = cacheSubscriptions.FirstOrDefault(s => s.Tenant.Equals(user.User));

                if (requiredSubscription != null && requiredSubscription.Name != null && requiredSubscription.Password != null)
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
