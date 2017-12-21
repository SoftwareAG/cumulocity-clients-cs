using Easy.MessageHub;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cumulocity.SDK.Microservices.Settings
{
    public static class PlatformExtensions
    {
        public static IServiceCollection AddPlatform(this IServiceCollection services, IConfiguration configuration)
        {         
            var settings = new PlatformSettings();

            new Microsoft.Extensions.Options.ConfigureFromConfigurationOptions<PlatformSettings>(configuration)
                .Configure(settings);

            services.AddSingleton(MessageHub.Instance);
            services.AddSingleton(new Platform(settings));
            return services;
        }
    }
}
