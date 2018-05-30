using System;
using System.Collections.Generic;
using System.Text;

namespace Cumulocity.SDK.Microservices.HealthCheck.Extentions
{
    public interface IHealthCheckResult
    {
        CheckStatus Status { get; }
        IReadOnlyDictionary<string, object> Data { get; }
    }
}
