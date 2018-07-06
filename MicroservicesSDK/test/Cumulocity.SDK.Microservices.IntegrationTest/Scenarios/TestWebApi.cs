
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using DemoWebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Xunit;
using System.Text;
using Baseline;
using Baseline.Reflection;
using Cumulocity.AspNetCore.Authentication.Basic;
using Cumulocity.SDK.Microservices.IntegrationTest.Client;
using Cumulocity.SDK.Microservices.IntegrationTest.Mocks;
using Cumulocity.SDK.Microservices.IntegrationTest.Utils;
using Cumulocity.SDK.Microservices.Model;
using Cumulocity.SDK.Microservices.Services;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cumulocity.SDK.Microservices.IntegrationTest.Scenarios
{
	public class ApplicationServiceTest : IClassFixture<TestFixture<StartupMock>>
	{
		public ApplicationServiceTest(TestFixture<StartupMock> fixture)
		{
			AppServiceClient = fixture.AppServiceClient;
			ApplicationServiceMock = fixture.ApplicationServiceMock;

			#region @Given

			var subscriptions = new List<Subscription>
			{
				new Subscription
				{
					Name = "UserName",
					Password = "UserPass",
					Tenant = "UserTenant"
				}
			};

			BasicAuthenticationResult basicAuthenticationResult = new BasicAuthenticationResult()
			{
				IsAuthenticated = true,
				Tenant = "UserTenant",
				Password = "UserPass",
				User = "UserName",

				Claims = new List<Claim>()
			};

			IList<User> users = new List<User>
			{
				new User
				{
					Name = "UserName",
				}
			};

			ApplicationServiceMock.Setup(x => x.GetCurrentApplicationSubscriptions())
				.Returns(Task.FromResult(subscriptions));

			ApplicationServiceMock.Setup(x => x.GetCurrentUser(It.IsAny<string>()))
				.Returns(Task.FromResult(basicAuthenticationResult));

			ApplicationServiceMock.Setup(x => x.GetUsers())
				.Returns(Task.FromResult(users));

			#endregion @Given
		}

		public ApplicationServicClient AppServiceClient { get; set; }

		public Mock<IApplicationService> ApplicationServiceMock { get; set; }

		public HttpClient Client { get; }

		[Fact]
		public async Task ApplicationService_GetCurrentApplicationSubscriptionsAsync()
		{
			//@Given
			AppServiceClient.IsAuthHeaderRequired = true;
			//@When
			var response = await AppServiceClient.GetSubscription();
			//@Then
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Single(response.Result);
			Assert.Equal("UserName", response.Result.First().Name);
			Assert.Equal("UserPass", response.Result.First().Password);
			Assert.Equal("UserTenant", response.Result.First().Tenant);
		}

		[Fact]
		public async Task ApplicationService_NotAuthentication_GetCurrentApplicationSubscriptionsAsync()
		{
			//@Given
			AppServiceClient.IsAuthHeaderRequired = false;
			//@When
			var response = await AppServiceClient.GetSubscription(false);
			//@Then
			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}

		[Fact]
		public async Task ApplicationService_GetCurrentUser()
		{
			//@Given
			AppServiceClient.IsAuthHeaderRequired = true;
			//@When
			var response = await AppServiceClient.GetCurrentUser();
			//@Then
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("UserName", response.Result.User);
			Assert.True(response.Result.IsAuthenticated);
			Assert.Equal("UserTenant", response.Result.Tenant);
			Assert.Equal("UserPass", response.Result.Password);
		}

		[Fact]
		public async Task ApplicationService_GetUsers()
		{
			//@Given
			AppServiceClient.IsAuthHeaderRequired = true;
			//@When
			var response = await AppServiceClient.GetUser();
			//@Then
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Single(response.Result);
			Assert.Equal("UserName", response.Result.First().Name);
		}
	}
}
