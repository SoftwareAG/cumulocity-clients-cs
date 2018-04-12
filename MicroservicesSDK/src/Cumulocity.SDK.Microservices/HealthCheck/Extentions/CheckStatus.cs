using System;
using System.Collections.Generic;
using System.Text;

namespace Cumulocity.SDK.Microservices.HealthCheck.Extentions
{
    public enum CheckStatus
    {
        Unknown,
        Unhealthy,
        Healthy,
        Warning
    }
}
