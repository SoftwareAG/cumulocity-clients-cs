using Cumulocity.AspNetCore.Authentication.Basic;
using Cumulocity.SDK.Microservices.Model;
using Cumulocity.SDK.Microservices.Settings;
using Cumulocity.SDK.Microservices.Utils.Extenions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Cumulocity.SDK.Microservices.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly ILogger _logger;
        private readonly Platform _platform;
        private const double requestTimeout = 2.0;

        public IHttpContextAccessor HttpContextAccessor { get; }

        public ApplicationService(ILogger<ApplicationService> logger, Platform platform, IHttpContextAccessor httpContextAccessor)
        {
            this._logger = logger;
            this._platform = platform;
            HttpContextAccessor = httpContextAccessor;
        }

        public async Task<List<Subscription>> GetCurrentApplicationSubscriptions()
        {
            List<Subscription> result = new List<Subscription>();
            var url = GetCurrentApplicationSubscriptionsUrl();
            var authCred = GetAuthCredentialsBase64(_platform.BOOTSTRAP_TENANT, _platform.BOOTSTRAP_USER, _platform.BOOTSTRAP_PASSWORD);

            var client = new System.Net.Http.HttpClient();
            client.Timeout = TimeSpan.FromSeconds(requestTimeout);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_platform.BASEURL + "/" + url)
            };

            request.Headers.Authorization =
                  new AuthenticationHeaderValue("Basic", authCred);

            var subscriptions = await client.SendAsync(request);

            if (subscriptions.IsSuccessStatusCode)
            {
                var jsonContent = await subscriptions.Content.ReadAsStringAsync();

                result = JsonConvert.DeserializeObject<CurrentApplicationSubscription>(jsonContent).Users;
            }
            return result;
        }

        public async Task<BasicAuthenticationResult> GetCurrentUser(string authCred)
        {
            var url = GetUrlCurrentUser();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_platform.BASEURL + url)
            };

            if (!string.IsNullOrEmpty(authCred))
                request.Headers.Authorization =
                  new AuthenticationHeaderValue("Basic", authCred);

            var client = new System.Net.Http.HttpClient();
            client.Timeout = TimeSpan.FromSeconds(2.0);

            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                UserRoles user = JsonConvert.DeserializeObject<UserRoles>(content, new UserRolesConverter());
                var claims = new List<Claim>();

                foreach (var role in user.Lists)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name, ClaimValueTypes.String, "Basic"));
                }

                return new BasicAuthenticationResult() { IsAuthenticated = true, Claims = claims };
            }
            return new BasicAuthenticationResult() { IsAuthenticated = false };
        }

        public async Task<IList<User>> GetUsers()
        {
            IList<User> result = new List<User>();

            string authCred = GetAuthCredentialsBase64(HttpContextAccessor.HttpContext.User.UserTenant(),
                                                       HttpContextAccessor.HttpContext.User.UserName(),
                                                       HttpContextAccessor.HttpContext.User.UserPassword());

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_platform.BASEURL + "/" + GetUserUsersUrl(HttpContextAccessor.HttpContext.User.UserTenant()))
            };

            request.Headers.Authorization =
                  new AuthenticationHeaderValue("Basic", authCred);

            var client = new System.Net.Http.HttpClient();
            client.Timeout = TimeSpan.FromSeconds(requestTimeout);

            var users = await client.SendAsync(request);
            if (users.IsSuccessStatusCode)
            {
                var jsonUsers = await users.Content.ReadAsStringAsync();

                dynamic usersObjects = JsonConvert.DeserializeObject(jsonUsers);

                foreach (var u in usersObjects.users)
                {
                    result.Add(new User { Name = u.userName, Enabled = u.enabled });
                }
            }

            return result;
        }

        #region Privates

        private static string GetUrlCurrentUser()
        {
            return "/user/currentUser";
        }

        private static string GetTenantFromHeader(string authHeader)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Split(' ')[1])).Split("/")[0];
        }

        private static string GetAuthCredentialsBase64(string tenant, string username, string password)
        {
            return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(tenant + "/" + username + ":" + password));
        }

        private static string GetCurrentApplicationSubscriptionsUrl()
        {
            return "application/currentApplication/subscriptions";
        }

        private static string GetUserUsersUrl(string tenant)
        {
            return $"user/{tenant}/users";
        }

        #endregion Privates
    }
}