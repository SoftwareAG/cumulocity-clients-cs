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
        private readonly IOAuthAuthenticationService _authenticationService;

        public OAuthAuthenticationHandler(
            IOptionsMonitor<OAuthAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IOAuthAuthenticationService authenticationService)
            : base(options, logger, encoder, clock)
        {
            _authenticationService = authenticationService;
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
            bool isValidUser = await _authenticationService.IsValidUserAsync(Options, Request.Headers);

            if (!isValidUser)
            {

                Logger.LogInformation("Failed to validate oAuth headers.");
                return AuthenticateResult.Fail("Failed to validate userid/password.");
            }
            Logger.LogInformation("Successfully validated oAuth headers.");



            var claims = new[] { new Claim(ClaimTypes.Name, "username") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}
