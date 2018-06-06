using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Cumulocity.AspNetCore.Authentication.Basic;
using Cumulocity.SDK.Microservices.FunctionalTest.Client;
using Cumulocity.SDK.Microservices.Model;
using Newtonsoft.Json;

namespace Cumulocity.SDK.Microservices.FunctionalTest
{
	public class ApplicationServicClient
	{
		private HttpClient _httpClient;

		public ApplicationServicClient(HttpClient httpClient)
		{
			this._httpClient = httpClient;
		}

		public async Task<ApiResponse<List<Subscription>>> GetSubscription(bool isAuth = true)
		{
			var subscriptions = await GetAsync<List<Subscription>>("/api/data/subscriptions", isAuth);
			return subscriptions;
		}

		public async Task<ApiResponse<BasicAuthenticationResult>> GetCurrentUser()
		{
			var currentuser = await GetAsync<BasicAuthenticationResult>("/api/data/currentuser");
			return currentuser;
		}

		public async Task<ApiResponse<IList<User>>> GetUser()
		{
			var users = await GetAsync<IList<User>>("/api/data/user");
			return users;
		}

		//IList<User> users

		private async Task<ApiResponse<T>> GetAsync<T>(string path, bool auth=true)
		{
			if (auth)
			{
				_httpClient.DefaultRequestHeaders.Authorization =
					new AuthenticationHeaderValue(
						"Basic",
						Convert.ToBase64String(
							System.Text.ASCIIEncoding.ASCII.GetBytes(
								string.Format("{0}:{1}", "tenant/username", "password"))));
			}
			else
			{
				_httpClient.DefaultRequestHeaders.Authorization = null;
			}

			var response = await _httpClient.GetAsync(path);
			var value = await response.Content.ReadAsStringAsync();
			var result = new ApiResponse<T>
			{
				StatusCode = response.StatusCode,
				ResultAsString = value
			};

			try
			{
				result.Result = JsonConvert.DeserializeObject<T>(value);
			}
			catch (Exception)
			{
				// Nothing to do
			}

			return result;
		}

	}
}