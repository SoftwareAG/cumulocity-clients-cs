using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cumulocity.SDK.Microservices.OAuth.Handler
{
    public class OAuthAuthenticationHandler : AuthenticationHandler<OAuthAuthenticationOptions>
    {
        private readonly IOAuthCredentialVerifier _authenticationVerifier;

        public OAuthAuthenticationHandler(
            IOAuthCredentialVerifier authCredentialVerifier,
            IOptionsMonitor<OAuthAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _authenticationVerifier = authCredentialVerifier;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            const string xXsrfToken = "X-XSRF-TOKEN";
            const string Cookie = "Cookie";

            if (!Request.Headers.ContainsKey(xXsrfToken))
            {
                return AuthenticateResult.NoResult();
            }
            if (!Request.Headers.ContainsKey(Cookie))
            {
                return AuthenticateResult.NoResult();
            }
            var validUser = await _authenticationVerifier.AuthenticateAsync(Options, Request.Headers);

            if (!validUser.IsAuthenticated)
            {
                Logger.LogInformation("Failed to validate oAuth headers.");
                return AuthenticateResult.Fail("Failed to validate userid/password.");
            }
            Logger.LogInformation("Successfully validated oAuth headers.");


            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, validUser.User, ClaimValueTypes.String, ClaimsIssuer),
                new Claim("UserName", validUser.User, ClaimValueTypes.String, ClaimsIssuer),
                new Claim("UserPassword", validUser.Password, ClaimValueTypes.String, ClaimsIssuer),
                new Claim("UserTenant", validUser.Tenant, ClaimValueTypes.String, ClaimsIssuer)
            };
            claims.AddRange(validUser.Claims);
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}
