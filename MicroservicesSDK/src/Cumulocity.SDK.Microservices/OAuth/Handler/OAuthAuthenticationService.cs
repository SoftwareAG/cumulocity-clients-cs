using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Cumulocity.SDK.Microservices.OAuth.Handler
{
    public class OAuthAuthenticationService : IOAuthAuthenticationService
    {
        public async Task<bool> IsValidUserAsync(OAuthAuthenticationOptions options, IHeaderDictionary headers)
        {
            //var url = GetUrlCurrentUser();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(@"http://oauth.staging-latest.c8y.io/user/currentUser")
            };

            //if (!string.IsNullOrEmpty(authCred))
            foreach (var header in headers)
            {
                if (header.Key.Equals("X-XSRF-TOKEN"))
                    request.Headers.TryAddWithoutValidation("x-xsrf-token", header.Value.FirstOrDefault());
                if (header.Key.Equals("Cookie"))
                    request.Headers.TryAddWithoutValidation("cookie", new List<string>(header.Value.Where(h => h.Contains("XSRF-TOKEN"))));
            }

            var client = new System.Net.Http.HttpClient();
            client.Timeout = TimeSpan.FromSeconds(2.0);

            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                //IsAuthenticated = true, Claims = claims ;
                return await Task.FromResult(true);
            }
            // IsAuthenticated = false ;
            return await Task.FromResult(false);
        }
    }
}
