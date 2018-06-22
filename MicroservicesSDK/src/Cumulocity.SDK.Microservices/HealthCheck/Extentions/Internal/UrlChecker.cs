using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Cumulocity.SDK.Microservices.HealthCheck.Internal;
using Newtonsoft.Json;

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
				    var request = createHttpRequestMessage(_url);
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

	    private HttpRequestMessage createHttpRequestMessage(string url)
	    {
			var request = new HttpRequestMessage
		    {
			    Method = HttpMethod.Get,
			    RequestUri = new Uri(url)
		    };

		    request.Headers.Authorization =
			    new AuthenticationHeaderValue("Basic", _auth);
		    return request;
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
            };

			string jsonString = response.Content.ReadAsStringAsync()
										.Result
										.Replace("\\", "")
										.Trim(new char[1] { '"' });

	        var ajsonObject = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonString);

	        foreach (var key in ajsonObject.Keys)
	        {
		        Console.WriteLine(key);
		        if (key.Equals("details"))
		        {
			        data = Dyn2Dict(ajsonObject["details"]);
		        }
	        }

			return HealthCheckResult.FromStatus(status, $"status code {response.StatusCode} ({(int)response.StatusCode})", data);
        }

	    public static Dictionary<String, Object> Dyn2Dict(dynamic dynObj)
	    {
		    var dictionary = new Dictionary<string, object>();
		    foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(dynObj))
		    {
			    object obj = propertyDescriptor.GetValue(dynObj);
			    dictionary.Add(propertyDescriptor.Name, obj);
		    }
		    return dictionary;
	    }


		protected virtual HttpClient GetHttpClient()
            => new HttpClient();
    }

	public class UrlCheckerWithDetails
	{
		private readonly Func<HttpResponseMessage, ValueTask<IHealthCheckResult>> _checkFunc;
		private readonly string _url;
		private readonly string _auth;
		private readonly PlatformHealthIndicatorProperties _configuration;
		public UrlCheckerWithDetails(Func<HttpResponseMessage, ValueTask<IHealthCheckResult>> checkFunc, string url, string auth)
		{
			Guard.ArgumentNotNull(nameof(checkFunc), checkFunc);
			Guard.ArgumentNotNullOrEmpty(nameof(url), url);
			Guard.ArgumentNotNullOrEmpty(nameof(auth), auth);

			_checkFunc = checkFunc;
			_url = url;
			_auth = auth;
			_configuration = new PlatformHealthIndicatorProperties();
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
					var request = createHttpRequestMessage(_url + _configuration.Path);
					var response = await httpClient.SendAsync(request).ConfigureAwait(false);

					if (response.IsSuccessStatusCode)
					{
						var requestDetails = createHttpRequestMessage(_url + _configuration.Details.Path);
						var responseDetails = await httpClient.SendAsync(requestDetails).ConfigureAwait(false);

						return await _checkFunc(responseDetails);
					}
					else
					{
						return await _checkFunc(response);
					}
				}
				catch (Exception ex)
				{
					var data = new Dictionary<string, object> { { "url", _url } };
					return HealthCheckResult.Unhealthy($"Exception during check: {ex.GetType().FullName}", data);
				}
			}
		}

		private HttpRequestMessage createHttpRequestMessage(string url)
		{
			var request = new HttpRequestMessage
			{
				Method = HttpMethod.Get,
				RequestUri = new Uri(url)
			};

			//request.Headers.Authorization =
			//	new AuthenticationHeaderValue("Basic", _auth);
			return request;
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
				//{ "url", response.RequestMessage.RequestUri.ToString() },
				//{ "status", (int)response.StatusCode },
				//{ "reason", response.ReasonPhrase },
				//{ "body", await response.Content?.ReadAsStringAsync() }
			};

			string jsonString = response.Content.ReadAsStringAsync()
				.Result
				.Replace("\\", "")
				.Trim(new char[1] { '"' });

			var ajsonObject = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonString);

			foreach (var key in ajsonObject.Keys)
			{
				Console.WriteLine(key);
				if (key.Equals("details"))
				{
					data = Dyn2Dict(ajsonObject["details"]);
				}
			}
			return HealthCheckResult.FromStatus(status, $"status code {response.StatusCode} ({(int)response.StatusCode})", data);
		}

		public static Dictionary<String, Object> Dyn2Dict(dynamic dynObj)
		{
			var dictionary = new Dictionary<string, object>();
			foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(dynObj))
			{
				object obj = propertyDescriptor.GetValue(dynObj);
				dictionary.Add(propertyDescriptor.Name, obj);
			}
			return dictionary;
		}


		protected virtual HttpClient GetHttpClient()
			=> new HttpClient();
	}

	public class PlatformHealthIndicatorProperties
	{
		public string Path { get; } = "/user/currentUser";

		public bool DetailsEnabled { get; } = true;
		public DetailsProperties Details { get; } 

		public PlatformHealthIndicatorProperties()
		{
			this.Details = new DetailsProperties(); 
		}
		public class DetailsProperties
		{

			public bool Enabled { get; } = true;
			public String Path { get; } = "/tenant/health"; 
		}
	}
}
