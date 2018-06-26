using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cumulocity.SDK.Microservices.HealthCheck.Internal;

namespace Cumulocity.SDK.Microservices.HealthCheck.Extentions
{
    public static class CachedHealthCheckExtensions
    {
        public static ValueTask<IHealthCheckResult> RunAsync(this CachedHealthCheck check, IServiceProvider serviceProvider)
        {
            Guard.ArgumentNotNull(nameof(check), check);

            return check.RunAsync(serviceProvider, CancellationToken.None);
        }
    }
}
