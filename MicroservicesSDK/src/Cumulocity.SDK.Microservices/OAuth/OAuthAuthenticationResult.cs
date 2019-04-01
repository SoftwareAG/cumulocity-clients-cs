using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Cumulocity.SDK.Microservices.OAuth
{
    public class OAuthAuthenticationResult
    {
        /// <summary>
        /// Gets a value indicating whether the request has been authenticated.
        /// </summary>
        public bool IsAuthenticated = false;

        /// <summary>
        /// Represents a list of claims
        /// </summary>
        public List<Claim> Claims = new List<Claim>();

        /// <summary>
        /// Represents a current user
        /// </summary>
        public string User = String.Empty;

        /// <summary>
        /// The password of the current user
        /// </summary>
        public string Password = String.Empty;

        /// <summary>
        /// The tenant of the current user
        /// </summary>
        public string Tenant = String.Empty;
    }
}
