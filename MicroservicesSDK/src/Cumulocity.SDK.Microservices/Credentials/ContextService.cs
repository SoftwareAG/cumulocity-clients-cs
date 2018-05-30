using System;
using System.Collections.Generic;
using System.Text;
using Cumulocity.SDK.Microservices.Settings;
using Cumulocity.SDK.Microservices.Utils.Extenions;
using Microsoft.AspNetCore.Http;

namespace Cumulocity.SDK.Microservices.Credentials
{
	public interface IContextService
	{
		ICredentials GetCredentials();
		string GetBaseUrl();
	}

	public class ContextService : IContextService
	{
		private readonly IHttpContextAccessor _contextAccessor;
		private readonly Platform _platform;

		public ContextService(IHttpContextAccessor ctx, Platform platform)
		{
			_contextAccessor = ctx;
			_platform = platform;
		}

		public ICredentials GetCredentials()
		{
			if (_contextAccessor.HttpContext != null && _contextAccessor.HttpContext.User.IsInContext())
			{
				return new MicroserviceCredentialsBuilder()
					.WithTenant(_contextAccessor.HttpContext.User.UserTenant())
					.WithUsername(_contextAccessor.HttpContext.User.UserName())
					.WithPassword(_contextAccessor.HttpContext.User.UserPassword())
					.Build();
			}

			return new MicroserviceCredentialsBuilder()
				.WithTenant(_platform.BOOTSTRAP_TENANT)
				.WithUsername(_platform.BOOTSTRAP_USER)
				.WithPassword(_platform.BOOTSTRAP_PASSWORD)
				.Build();
		}

		public string GetBaseUrl()
		{
			return _platform.BASEURL;
		}

	}
}
