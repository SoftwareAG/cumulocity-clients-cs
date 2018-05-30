using System;
using System.Collections.Generic;
using System.Text;

namespace Cumulocity.SDK.Microservices.HealthCheck.Extentions
{
    public class HealthCheckResult : IHealthCheckResult
    {
        private static readonly IReadOnlyDictionary<string, object> _emptyData = new Dictionary<string, object>();

        public CheckStatus Status { get; }
        public IReadOnlyDictionary<string, object> Data { get; }
        public string Description { get; }

        private HealthCheckResult(CheckStatus checkStatus, string description, IReadOnlyDictionary<string, object> data)
        {
            Status = checkStatus;
            Description = description;
            Data = data ?? _emptyData;
        }

        public static HealthCheckResult Unhealthy(string description)
            => new HealthCheckResult(CheckStatus.Unhealthy, description, null);

        public static HealthCheckResult Unhealthy(string description, IReadOnlyDictionary<string, object> data)
            => new HealthCheckResult(CheckStatus.Unhealthy, description, data);

        public static HealthCheckResult Healthy(string description)
            => new HealthCheckResult(CheckStatus.Healthy, description, null);

        public static HealthCheckResult Healthy(string description, IReadOnlyDictionary<string, object> data)
            => new HealthCheckResult(CheckStatus.Healthy, description, data);

        public static HealthCheckResult Warning(string description)
            => new HealthCheckResult(CheckStatus.Warning, description, null);

        public static HealthCheckResult Warning(string description, IReadOnlyDictionary<string, object> data)
            => new HealthCheckResult(CheckStatus.Warning, description, data);

        public static HealthCheckResult Unknown(string description)
            => new HealthCheckResult(CheckStatus.Unknown, description, null);

        public static HealthCheckResult Unknown(string description, IReadOnlyDictionary<string, object> data)
            => new HealthCheckResult(CheckStatus.Unknown, description, data);

        public static HealthCheckResult FromStatus(CheckStatus status, string description)
            => new HealthCheckResult(status, description, null);

        public static HealthCheckResult FromStatus(CheckStatus status, string description, IReadOnlyDictionary<string, object> data)
            => new HealthCheckResult(status, description, data);
    }
}
