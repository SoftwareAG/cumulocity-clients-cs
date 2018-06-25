using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Cumulocity.SDK.Microservices.FunctionalTest.Mocks;
using Cumulocity.SDK.Microservices.FunctionalTest.Utils;
using DemoWebApi;
using Xunit;

namespace Cumulocity.SDK.Microservices.FunctionalTest.Scenarios
{
	public class ApplicationServiceTestLive : IClassFixture<TestFixtureLive<Startup>>
	{
		public ApplicationServiceTestLive(TestFixtureLive<Startup> fixture)
		{
			AppServiceClient = fixture.AppServiceClient;
		}

		public ApplicationServicClient AppServiceClient { get; }

		[Fact]
		public async Task ApplicationService_GetCurrentApplicationSubscriptionsAsync()
		{
			var response = await AppServiceClient.GetSubscription();

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Single(response.Result);
			Assert.Equal("UserName", response.Result.First().Name);
			Assert.Equal("UserPass", response.Result.First().Password);
			Assert.Equal("UserTenant", response.Result.First().Tenant);
		}

		[Fact]
		public async Task ApplicationService_NotAuthentication_GetCurrentApplicationSubscriptionsAsync()
		{
			var response = await AppServiceClient.GetSubscription(false);

			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}

		[Fact]
		public async Task ApplicationService_GetCurrentUser()
		{
			var response = await AppServiceClient.GetCurrentUser();

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("UserName", response.Result.User);
			Assert.True(response.Result.IsAuthenticated);
			Assert.Equal("UserTenant", response.Result.Tenant);
			Assert.Equal("UserPass", response.Result.Password);
		}

		[Fact]
		public async Task ApplicationService_GetUsers()
		{
			var response = await AppServiceClient.GetUser();
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Single(response.Result);
			Assert.Equal("UserName", response.Result.First().Name);
		}
	}
}
