using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Cumulocity.SDK.Microservices.HealthCheck.Extentions.Internal;
using Cumulocity.SDK.Microservices.HealthCheck.Internal;

namespace Cumulocity.SDK.Microservices.HealthCheck.Extentions.Checks
{
    public static partial class HealthCheckBuilderExtensions
    {
        // Default URL check

        public static HealthCheckBuilder AddUrlCheck(this HealthCheckBuilder builder, string url)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            return AddUrlCheck(builder, url, builder.DefaultCacheDuration);
        }

        public static HealthCheckBuilder AddUrlCheck(this HealthCheckBuilder builder, string url, TimeSpan cacheDuration)
            => AddUrlCheck(builder, url, response => UrlChecker.DefaultUrlCheck(response), cacheDuration);

        // Func returning IHealthCheckResult

        public static HealthCheckBuilder AddUrlCheck(this HealthCheckBuilder builder, string url, Func<HttpResponseMessage, IHealthCheckResult> checkFunc)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            return AddUrlCheck(builder, url, checkFunc, builder.DefaultCacheDuration);
        }

        public static HealthCheckBuilder AddUrlCheck(this HealthCheckBuilder builder, string url,
                                                     Func<HttpResponseMessage, IHealthCheckResult> checkFunc,
                                                     TimeSpan cacheDuration)
        {
            Guard.ArgumentNotNull(nameof(checkFunc), checkFunc);

            return AddUrlCheck(builder, url, response => new ValueTask<IHealthCheckResult>(checkFunc(response)), cacheDuration);
        }

        // Func returning Task<IHealthCheckResult>

        public static HealthCheckBuilder AddUrlCheck(this HealthCheckBuilder builder, string url, Func<HttpResponseMessage, Task<IHealthCheckResult>> checkFunc)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            return AddUrlCheck(builder, url, checkFunc, builder.DefaultCacheDuration);
        }

        public static HealthCheckBuilder AddUrlCheck(this HealthCheckBuilder builder, string url,
                                                     Func<HttpResponseMessage, Task<IHealthCheckResult>> checkFunc,
                                                     TimeSpan cacheDuration)
        {
            Guard.ArgumentNotNull(nameof(checkFunc), checkFunc);

            return AddUrlCheck(builder, url, response => new ValueTask<IHealthCheckResult>(checkFunc(response)), cacheDuration);
        }

        // Func returning ValueTask<IHealthCheckResult>

        public static HealthCheckBuilder AddUrlCheck(this HealthCheckBuilder builder, string url, Func<HttpResponseMessage, ValueTask<IHealthCheckResult>> checkFunc)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            return AddUrlCheck(builder, url, checkFunc, builder.DefaultCacheDuration);
        }

        public static HealthCheckBuilder AddUrlCheck(this HealthCheckBuilder builder, string url,
                                                     Func<HttpResponseMessage, ValueTask<IHealthCheckResult>> checkFunc,
                                                     TimeSpan cacheDuration)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);
            Guard.ArgumentNotNullOrEmpty(nameof(url), url);
            Guard.ArgumentNotNull(nameof(checkFunc), checkFunc);

            var urlCheck = new UrlChecker(checkFunc, url);
            builder.AddCheck($"UrlCheck({url})", () => urlCheck.CheckAsync(), cacheDuration);
            return builder;
        }
    }
}
