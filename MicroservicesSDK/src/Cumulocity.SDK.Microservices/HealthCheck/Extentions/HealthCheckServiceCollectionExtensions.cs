using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cumulocity.SDK.Microservices.HealthCheck.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Cumulocity.SDK.Microservices.HealthCheck.Extentions
{
    public static class HealthCheckServiceCollectionExtensions
    {
        private static readonly Type HealthCheckServiceInterface = typeof(IHealthCheckService);

        public static IServiceCollection AddHealthChecks(this IServiceCollection services, Action<HealthCheckBuilder> checks)
        {
            Guard.OperationValid(!services.Any(descriptor => descriptor.ServiceType == HealthCheckServiceInterface), "AddHealthChecks may only be called once.");

            var builder = new HealthCheckBuilder();

	    

			services.AddSingleton<IHealthCheckService, HealthCheckService>(serviceProvider =>
            {
                var serviceScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
	            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
				return new HealthCheckService(builder, serviceProvider, serviceScopeFactory, httpContextAccessor);
            });

            checks(builder);
            return services;
        }

	    public static IServiceCollection AddHealthChecks(this IServiceCollection services, Action<HealthCheckBuilder> checks, TimeSpan interval)
	    {
		    Guard.OperationValid(!services.Any(descriptor => descriptor.ServiceType == HealthCheckServiceInterface), "AddHealthChecks may only be called once.");

		    var builder = new HealthCheckBuilder();

		    services.AddSingleton<IHealthCheckService, HealthCheckService>(serviceProvider =>
		    {
			    var serviceScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
			    var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
				return new HealthCheckService(builder, serviceProvider, serviceScopeFactory, httpContextAccessor);
		    });

		    checks(builder);
		    return services;
	    }
	}
}
