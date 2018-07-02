using System;
using System.Collections.Generic;
using System.Text;

namespace Cumulocity.SDK.Microservices.HealthCheck.Extentions
{
    public class HealthCheckResults
    {
        public IList<IHealthCheckResult> CheckResults { get; } = new List<IHealthCheckResult>();
    }
}
