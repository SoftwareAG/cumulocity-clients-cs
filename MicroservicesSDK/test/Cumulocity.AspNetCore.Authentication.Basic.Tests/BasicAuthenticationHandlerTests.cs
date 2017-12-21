using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Net.Http;
using System.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Cumulocity.AspNetCore.Authentication.Basic.Tests
{
    public class BasicAuthenticationHandlerTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("SomeRealm")]
        [InlineData("AnotherRealm")]
        public async Task NoAuthorizeOnRequestReturnsChallenge(string expectedRealm)
        {
            var client = TestBed.GetClient(o => o.Realm = expectedRealm);
            var response = await client.GetAsync("/");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
            Assert.Equal($"Basic realm=\"{expectedRealm}\"", response.Headers.WwwAuthenticate.Single().ToString());
        }

        [Fact]
        public async Task DefaultTestBedRejectsCredentials()
        {
            var client = TestBed.GetClient();
            client.SetBasic("user", "pass");
            var response = await client.GetAsync("/");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
            Assert.Equal("Basic realm=\"\"", response.Headers.WwwAuthenticate.Single().ToString());
        }

        [Fact]
        public async Task ValidCredentialsAuthorize()
        {
            const string username = "management/admin";
            const string password = "Pyi1bo1r";
            //var client = TestBed.GetClient(builder => builder.AddBasicAuthentication(
            //    userPass => Task.FromResult(new BasicAuthenticationResult() { IsAuthenticated = (userPass.username == username && userPass.password == password) })));

            var client = TestBed.GetClient(builder => builder.AddBasicAuthentication<BasicCredentialVerifier>());
            client.SetBasic(username, password);
            var response = await client.GetAsync("/");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
            Assert.False(response.Headers.WwwAuthenticate.Any());
        }


        internal class BasicCredentialVerifier : IBasicCredentialVerifier
        {
            public Task<BasicAuthenticationResult> Authenticate(string username, string password)
            {
                return GetCurrentUserAsync(username, password);
            }

            private async Task<BasicAuthenticationResult> GetCurrentUserAsync(string username, string password)
            {
                var authCred = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(username + ":" + password));

                var url = "/user/currentUser";
                var baseURL = "http://management.staging7.c8y.io";

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(baseURL + url)
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
                    var claims = new List<Claim>();
                    claims.Add(new Claim(ClaimTypes.Role, "Administrator", ClaimValueTypes.String, "Basic"));
                    return new BasicAuthenticationResult() { IsAuthenticated = true, Claims = claims };
                }
                return new BasicAuthenticationResult() { IsAuthenticated = false };

            }
        }
    }
}
