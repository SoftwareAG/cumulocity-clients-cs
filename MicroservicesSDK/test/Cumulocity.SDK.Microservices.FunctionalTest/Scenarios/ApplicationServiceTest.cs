#region Cumulocity GmbH

// /*
//  * Copyright (C) 2015-2018
//  *
//  * Permission is hereby granted, free of charge, to any person obtaining a copy of
//  * this software and associated documentation files (the "Software"),
//  * to deal in the Software without restriction, including without limitation the rights to use,
//  * copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
//  * and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//  *
//  * The above copyright notice and this permission notice shall be
//  * included in all copies or substantial portions of the Software.
//  *
//  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//  * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
//  * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//  * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
//  * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
//  * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  */

#endregion

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Cumulocity.AspNetCore.Authentication.Basic;
using Cumulocity.SDK.Microservices.FunctionalTest.Mocks;
using Cumulocity.SDK.Microservices.Model;
using Cumulocity.SDK.Microservices.Services;
using Moq;
using Xunit;

namespace Cumulocity.SDK.Microservices.FunctionalTest.Scenarios
{
	public class ApplicationServiceTest : IClassFixture<TestFixture<StartupMock>>
	{
		public ApplicationServiceTest(TestFixture<StartupMock> fixture)
		{
			AppServiceClient = fixture.AppServiceClient;
			ApplicationServiceMock = fixture.ApplicationServiceMock;

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
		}

		public ApplicationServicClient AppServiceClient { get; set; }

		public Mock<IApplicationService> ApplicationServiceMock { get; set; }

		public HttpClient Client { get; }

		[Fact]
		public async Task ApplicationService_GetCurrentApplicationSubscriptionsAsync()
		{
			var response = await AppServiceClient.GetSubscription();

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Single(response.Result);
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
		}

		[Fact]
		public async Task ApplicationService_GetUsers()
		{
			var response = await AppServiceClient.GetUser();
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Single(response.Result);
		}
	}
}