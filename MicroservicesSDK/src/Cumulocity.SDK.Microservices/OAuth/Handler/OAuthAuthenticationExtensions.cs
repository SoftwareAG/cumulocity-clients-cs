using System;
using System.Collections.Generic;
using System.Text;
using Cumulocity.SDK.Microservices.Services.OAuth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cumulocity.SDK.Microservices.OAuth.Handler
{
    public static class OAuthAuthenticationExtensions
    {
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

        public static AuthenticationBuilder AddOAuthAuthentication<T>(this AuthenticationBuilder builder)
            where T : class, IOAuthCredentialVerifier
        {
            builder.Services.TryAdd(new ServiceDescriptor(typeof(IOAuthApplicationService), typeof(OAuthApplicationService), ServiceLifetime.Transient));
            builder.Services.TryAdd(new ServiceDescriptor(typeof(IOAuthCredentialVerifier), typeof(T), ServiceLifetime.Transient));
            return builder.AddScheme<OAuthAuthenticationOptions, OAuthAuthenticationHandler>(OAuthAuthenticationDefaults.AuthenticationScheme, null);
        }
    }
}
