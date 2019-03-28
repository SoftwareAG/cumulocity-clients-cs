using Microsoft.AspNetCore.Authentication;

namespace Cumulocity.SDK.Microservices.OAuth.Handler
{
    public class OAuthAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string Scope { get; set; }
    }
}
