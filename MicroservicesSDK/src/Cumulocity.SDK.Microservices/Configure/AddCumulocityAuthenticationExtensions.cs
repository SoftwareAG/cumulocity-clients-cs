using Cumulocity.AspNetCore.Authentication.Basic;
using Cumulocity.SDK.Microservices.BasicAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Cumulocity.SDK.Microservices.OAuth;
using Cumulocity.SDK.Microservices.OAuth.Handler;
using Cumulocity.SDK.Microservices.Services.OAuth;
using Microsoft.AspNetCore.Authentication;

namespace Cumulocity.SDK.Microservices.Configure
{
    public static class AddCumulocityAuthenticationExtensions
    {
        public static IServiceCollection AddCumulocityAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
                .AddBasicAuthentication<BasicCredentialVerifier>();
            return services;
        }

        public static IServiceCollection AddCumulocityAuthenticationAll(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(OAuthAuthenticationDefaults.AuthenticationScheme)
                .AddBasicAuthentication<BasicCredentialVerifier>()
                .AddOAuthAuthentication<OAuthVerifier>();
            return services;
        }
    }
}
