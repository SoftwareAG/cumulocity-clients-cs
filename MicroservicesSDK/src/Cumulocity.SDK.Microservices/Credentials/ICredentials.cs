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
}
