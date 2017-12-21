using Cumulocity.AspNetCore.Authentication.Basic;
using Cumulocity.SDK.Microservices.BasicAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cumulocity.SDK.Microservices.Configure
{
    public static class AddCumulocityAuthenticationExtensions
    {
        public static IServiceCollection AddCumulocityAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme).AddBasicAuthentication<BasicCredentialVerifier>();
            return services;
        }
    }
}
