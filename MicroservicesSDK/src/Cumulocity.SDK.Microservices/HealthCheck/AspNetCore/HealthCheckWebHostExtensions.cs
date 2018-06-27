using System;
using Microsoft.AspNetCore.Hosting;
using Cumulocity.SDK.Microservices.HealthCheck.Extentions;

namespace Microsoft.AspNetCore.Hosting
{
    public static class HealthCheckWebHostExtensions
    {
        private const int DEFAULT_TIMEOUT_SECONDS = 300;

        public static void RunWhenHealthy(this IWebHost webHost)
        {
            webHost.RunWhenHealthy(TimeSpan.FromSeconds(DEFAULT_TIMEOUT_SECONDS));
        }

        public static void RunWhenHealthy(this IWebHost webHost, TimeSpan timeout)
        {
            var healthChecks = webHost.Services.GetService(typeof(IHealthCheckService)) as IHealthCheckService;

	        var currentTime = DateTime.Now;
	        var currentEndTime = currentTime.Add(timeout);

			var loops = 0;
			do
			{
				var checkResult = healthChecks.CheckHealthAsync().Result;
				if (checkResult.Status == CheckStatus.Healthy)
				{
					webHost.Run();
					break;
				}

				System.Threading.Thread.Sleep(100);
				loops++;
				currentTime = DateTime.Now;

			} while (currentEndTime < currentTime);

		}
	}
}
