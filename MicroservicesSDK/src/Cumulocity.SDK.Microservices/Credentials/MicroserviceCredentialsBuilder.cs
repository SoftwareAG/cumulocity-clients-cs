using System;
using System.Collections.Generic;
using System.Text;

namespace Cumulocity.SDK.Microservices.Credentials
{
	public class MicroserviceCredentialsBuilder
	{
		private string _tenant;
		private string _username;
		private string _password;

		public MicroserviceCredentialsBuilder WithTenant(string tenant)
		{
			this._tenant = tenant;
			return this;
		}
		public MicroserviceCredentialsBuilder WithUsername(string username)
		{
			this._username = username;
			return this;
		}
		public MicroserviceCredentialsBuilder WithPassword(string password)
		{
			this._password = password;
			return this;
		}

		public MicroserviceCredentials Build()
		{
			return new MicroserviceCredentials(_tenant, _username, _password, String.Empty, String.Empty);
		}
	}
}
