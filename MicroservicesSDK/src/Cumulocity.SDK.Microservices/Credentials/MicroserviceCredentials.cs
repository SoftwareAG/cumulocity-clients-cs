using System;
using System.Collections.Generic;
using System.Text;

namespace Cumulocity.SDK.Microservices.Credentials
{
	public class MicroserviceCredentials : ICredentials
	{
		private string _tenant;
		private string _username;
		private string _password;
		private string _tfaToken;
		private string _appKey;
		public string Username => _username;

		public string Tenant => _tenant;

		public string Password => _password;

		public string TfaToken => _tfaToken;

		public string AppKey => _appKey;

		public MicroserviceCredentials(string tenant, string username, string password, string tfaToken, string appKey)
		{
			_tenant = tenant;
			_username = username;
			_password = password;
			_tfaToken = tfaToken;
			_appKey = appKey;
		}
	}
}
