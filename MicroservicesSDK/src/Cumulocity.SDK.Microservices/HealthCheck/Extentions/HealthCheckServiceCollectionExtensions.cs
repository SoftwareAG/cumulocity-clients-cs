using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Cumulocity.SDK.Microservices.Credentials;
using Cumulocity.SDK.Microservices.HealthCheck.Internal;
using Cumulocity.SDK.Microservices.Settings;
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

			services.AddSingleton<IContextService, ContextService>(serviceProvider =>
			{
				var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
				var platform = (Platform)serviceProvider.GetRequiredService(typeof(Platform));
				return new ContextService(httpContextAccessor, platform);
			});

			services.AddSingleton<IHealthCheckService, HealthCheckService>(serviceProvider =>
            {
                var serviceScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
				var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
				return new HealthCheckService(builder, serviceProvider, serviceScopeFactory, httpContextAccessor);
            });

	        var provider = services.BuildServiceProvider();
	        var httpContext = provider.GetService<IHttpContextAccessor>();
	        var platformObj = (Platform)provider.GetRequiredService(typeof(Platform));
	        builder.WithContextService(new ContextService(httpContext, platformObj));
			checks(builder);
            return services;
        }

	    public static IServiceCollection AddHealthChecks(this IServiceCollection services, Action<HealthCheckBuilder> checks, TimeSpan interval)
	    {
		    Guard.OperationValid(!services.Any(descriptor => descriptor.ServiceType == HealthCheckServiceInterface), "AddHealthChecks may only be called once.");

		    var builder = new HealthCheckBuilder();

			services.AddSingleton<IContextService, ContextService>(serviceProvider =>
		    {
			    var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
			    var platform = (Platform) serviceProvider.GetRequiredService(typeof(Platform));
				return new ContextService(httpContextAccessor, platform);
		    });

			services.AddSingleton<IHealthCheckService, HealthCheckService>(serviceProvider =>
		    {
			    var serviceScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
			    var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
				return new HealthCheckService(builder, serviceProvider, serviceScopeFactory, httpContextAccessor);
		    });

		    var provider = services.BuildServiceProvider();
		    var httpContext = provider.GetService<IHttpContextAccessor>();
		    var platformObj = (Platform)provider.GetRequiredService(typeof(Platform));
		    builder.WithContextService(new ContextService(httpContext, platformObj));

			checks(builder);
		    return services;
	    }
	}
}
