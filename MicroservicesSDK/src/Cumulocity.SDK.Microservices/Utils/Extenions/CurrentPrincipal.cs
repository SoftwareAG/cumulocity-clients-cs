using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace Cumulocity.SDK.Microservices.Utils.Extenions
{
    public static class CurrentPrincipal
    {
        public static string GetSpecificClaim(this ClaimsIdentity claimsIdentity, string claimType)
        {
            var claim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == claimType);

            return (claim != null) ? claim.Value : string.Empty;
        }
    }

    public static class GenericPrincipalExtensions
    {
        public static string UserName(this IPrincipal user)
        {
            if (user.Identity.IsAuthenticated)
            {
                ClaimsIdentity claimsIdentity = user.Identity as ClaimsIdentity;
                foreach (var claim in claimsIdentity.Claims)
                {
                    if (claim.Type == "UserName")
                        return claim.Value;
                }
                return "";
            }
            else
                return "";
        }

        public static string UserPassword(this IPrincipal user)
        {
            if (user.Identity.IsAuthenticated)
            {
                ClaimsIdentity claimsIdentity = user.Identity as ClaimsIdentity;
	            if (claimsIdentity != null)
		            foreach (var claim in claimsIdentity.Claims)
		            {
			            if (claim.Type == "UserPassword")
				            return claim.Value;
		            }

	            return "";
            }
            else
                return "";
        }

        public static string UserTenant(this IPrincipal user)
        {
            if (user.Identity.IsAuthenticated)
            {
                ClaimsIdentity claimsIdentity = user.Identity as ClaimsIdentity;
	            if (claimsIdentity != null)
		            foreach (var claim in claimsIdentity.Claims)
		            {
			            if (claim.Type == "UserTenant")
				            return claim.Value;
		            }

	            return "";
            }
            else
                return "";
        }

	    public static bool IsInContext(this IPrincipal user)
	    {
		    

			if (user.Identity.IsAuthenticated)
		    {
			    var userName = String.Empty;
			    var userPassword = String.Empty;
				var userTenant = String.Empty;

				ClaimsIdentity claimsIdentity = user.Identity as ClaimsIdentity;
			    if (claimsIdentity != null)
				    foreach (var claim in claimsIdentity.Claims)
				    {
					    if (claim.Type == "UserTenant")
						    userName = claim.Value;
					    if (claim.Type == "UserTenant")
						    userPassword = claim.Value;
					    if (claim.Type == "UserTenant")
						    userTenant = claim.Value;
				    }

			    return !String.IsNullOrEmpty(userName) &&
			           !String.IsNullOrEmpty(userPassword) &&
			           !String.IsNullOrEmpty(userTenant);
		    }
			return false;

	    }


	}
}
