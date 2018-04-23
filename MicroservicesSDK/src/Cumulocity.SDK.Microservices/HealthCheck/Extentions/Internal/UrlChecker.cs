﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Cumulocity.SDK.Microservices.HealthCheck.Internal;

namespace Cumulocity.SDK.Microservices.HealthCheck.Extentions.Internal
{
    public class UrlChecker
    {
        private readonly Func<HttpResponseMessage, ValueTask<IHealthCheckResult>> _checkFunc;
        private readonly string _url;
	    private readonly string _auth;

	    public UrlChecker(Func<HttpResponseMessage, ValueTask<IHealthCheckResult>> checkFunc, string url)
        {
            Guard.ArgumentNotNull(nameof(checkFunc), checkFunc);
            Guard.ArgumentNotNullOrEmpty(nameof(url), url);

            _checkFunc = checkFunc;
            _url = url;
        }

	    public UrlChecker(Func<HttpResponseMessage, ValueTask<IHealthCheckResult>> checkFunc, string url, string auth)
	    {
		    Guard.ArgumentNotNull(nameof(checkFunc), checkFunc);
		    Guard.ArgumentNotNullOrEmpty(nameof(url), url);
		    Guard.ArgumentNotNullOrEmpty(nameof(auth), auth);

			_checkFunc = checkFunc;
		    _url = url;
		    _auth = auth;
	    }

		public CheckStatus PartiallyHealthyStatus { get; set; } = CheckStatus.Warning;

        public async Task<IHealthCheckResult> CheckAsync()
        {
            using (var httpClient = CreateHttpClient())
            {
                try
                {
                    var response = await httpClient.GetAsync(_url).ConfigureAwait(false);
                    return await _checkFunc(response);
                }
                catch (Exception ex)
                {
                    var data = new Dictionary<string, object> { { "url", _url } };
                    return HealthCheckResult.Unhealthy($"Exception during check: {ex.GetType().FullName}", data);
                }
            }
        }

	    public async Task<IHealthCheckResult> CheckWithBasicAuthAsync()
	    {
		    using (var httpClient = CreateHttpClient())
		    {
			    try
			    {
				    var request = new HttpRequestMessage
				    {
					    Method = HttpMethod.Get,
					    RequestUri = new Uri(_url)
				    };

				    request.Headers.Authorization =
					    new AuthenticationHeaderValue("Basic", _auth);

					var response = await httpClient.SendAsync(request).ConfigureAwait(false);
				    return await _checkFunc(response);
			    }
			    catch (Exception ex)
			    {
				    var data = new Dictionary<string, object> { { "url", _url } };
				    return HealthCheckResult.Unhealthy($"Exception during check: {ex.GetType().FullName}", data);
			    }
		    }
	    }

		private HttpClient CreateHttpClient()
        {
            var httpClient = GetHttpClient();
            httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            return httpClient;
        }

		public static async ValueTask<IHealthCheckResult> DefaultUrlCheck(HttpResponseMessage response)
        {
            var status = response.IsSuccessStatusCode ? CheckStatus.Healthy : CheckStatus.Unhealthy;
            var data = new Dictionary<string, object>
            {
                { "url", response.RequestMessage.RequestUri.ToString() },
                { "status", (int)response.StatusCode },
                { "reason", response.ReasonPhrase },
                { "body", await response.Content?.ReadAsStringAsync() }
            };
            return HealthCheckResult.FromStatus(status, $"status code {response.StatusCode} ({(int)response.StatusCode})", data);
        }

        protected virtual HttpClient GetHttpClient()
            => new HttpClient();
    }
}
