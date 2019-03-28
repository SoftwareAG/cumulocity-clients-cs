using System;
using System.Collections.Generic;
using System.Text;

namespace Cumulocity.SDK.Microservices.OAuth.Handler
{
    public static class OAuthAuthenticationExtensions
    {
        public static AuthenticationBuilder AddOAuthAuthentication<T>(this AuthenticationBuilder builder)
            where T : class, IOAuthAuthenticationService
        {
            return AddOAuthAuthentication<T>(builder, OAuthAuthenticationDefaults.AuthenticationScheme, _ => { });
        }

        public static AuthenticationBuilder AddOAuthAuthentication<T>(this AuthenticationBuilder builder, string authenticationScheme)
            where T : class, IOAuthAuthenticationService
        {
            return AddOAuthAuthentication<T>(builder, authenticationScheme, _ => { });
        }

        public static AuthenticationBuilder AddOAuthAuthentication<T>(this AuthenticationBuilder builder, Action<OAuthAuthenticationOptions> configureOptions)
            where T : class, IOAuthAuthenticationService
        {
            return AddOAuthAuthentication<T>(builder, OAuthAuthenticationDefaults.AuthenticationScheme, configureOptions);
        }

        public static AuthenticationBuilder AddOAuthAuthentication<T>(this AuthenticationBuilder builder, string authenticationScheme, Action<OAuthAuthenticationOptions> configureOptions)
            where T : class, IOAuthAuthenticationService
        {
            builder.Services.AddTransient<IOAuthAuthenticationService, T>();

            return builder.AddScheme<OAuthAuthenticationOptions, OAuthAuthenticationHandler>(authenticationScheme, configureOptions);
        }
    }
}
