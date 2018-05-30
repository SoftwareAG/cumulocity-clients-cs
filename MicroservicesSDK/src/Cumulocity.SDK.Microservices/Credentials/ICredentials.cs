using System;
using System.Collections.Generic;
using System.Text;

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
