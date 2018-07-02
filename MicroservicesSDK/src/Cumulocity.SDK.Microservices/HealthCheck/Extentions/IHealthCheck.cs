using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cumulocity.SDK.Microservices.HealthCheck.Extentions
{
    public interface IHealthCheck
    {
        ValueTask<IHealthCheckResult> CheckAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
