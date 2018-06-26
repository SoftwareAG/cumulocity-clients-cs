using Cumulocity.SDK.Microservices.Settings;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using Cumulocity.SDK.Microservices.Utils.Extenions;

namespace Cumulocity.SDK.Microservices.Credentials
{
	public interface ICredentials
	{
		String Username { get; }
		String Tenant { get; }
		String Password { get; }
		String TfaToken { get; }
		String AppKey { get; }
	}

	//public class MicroserviceCredentials : ICredentials
	//{
	//	private string _tenant;
	//	private string _username;
	//	private string _password;
	//	private string _tfaToken;
	//	private string _appKey;
	//	public string Username => _username;

	//	public string Tenant => _tenant;

	//	public string Password => _password;

	//	public string TfaToken => _tfaToken;

	//	public string AppKey => _appKey;

	//	public MicroserviceCredentials(string tenant, string username, string password, string tfaToken, string appKey)
	//	{
	//		_tenant = tenant;
	//		_username = username;
	//		_password = password;
	//		_tfaToken = tfaToken;
	//		_appKey = appKey;
	//	}
	//}

	//public class MicroserviceCredentialsBuilder
	//{
	//	private string _tenant;
	//	private string _username;
	//	private string _password;

	//	public MicroserviceCredentialsBuilder WithTenant(string tenant)
	//	{
	//		this._tenant = tenant;
	//		return this;
	//	}
	//	public MicroserviceCredentialsBuilder WithUsername(string username)
	//	{
	//		this._username = username;
	//		return this;
	//	}
	//	public MicroserviceCredentialsBuilder WithPassword(string password)
	//	{
	//		this._password = password;
	//		return this;
	//	}

	//	public MicroserviceCredentials Build()
	//	{
	//		return new MicroserviceCredentials(_tenant, _username, _password, String.Empty, String.Empty);
	//	}
	//}

	//public class ContextService
	//{
	//	private IHttpContextAccessor _contextAccessor;
	//	private Platform _platform;

	//	public ContextService(IHttpContextAccessor ctx, Platform platform)
	//	{
	//		_contextAccessor = ctx;
	//		_platform = platform;
	//	}

	//	private ICredentials GetCredentials()
	//	{
	//		if (_contextAccessor.HttpContext.User.IsInContext())
	//		{
	//			return new MicroserviceCredentialsBuilder()
	//						.WithTenant(_contextAccessor.HttpContext.User.UserTenant())
	//						.WithUsername(_contextAccessor.HttpContext.User.UserName())
	//						.WithPassword(_contextAccessor.HttpContext.User.UserPassword()).Build();
	//		}

	//		return new MicroserviceCredentialsBuilder()
	//					.WithTenant(_platform.BOOTSTRAP_TENANT)
	//					.WithUsername(_platform.BOOTSTRAP_USER)
	//			        .WithPassword(_platform.BOOTSTRAP_PASSWORD).Build();
	//	}
	//}
}
