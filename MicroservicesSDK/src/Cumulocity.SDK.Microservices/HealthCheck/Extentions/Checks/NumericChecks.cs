using System;
using System.Collections.Generic;
using System.Text;
using Cumulocity.SDK.Microservices.HealthCheck.Internal;

namespace Cumulocity.SDK.Microservices.HealthCheck.Extentions.Checks
{
    public static partial class HealthCheckBuilderExtensions
    {
        // Numeric checks

        public static HealthCheckBuilder AddMinValueCheck<T>(this HealthCheckBuilder builder, string name, T minValue, Func<T> currentValueFunc) where T : IComparable<T>
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            return AddMinValueCheck(builder, name, minValue, currentValueFunc, builder.DefaultCacheDuration);
        }

        public static HealthCheckBuilder AddMinValueCheck<T>(this HealthCheckBuilder builder, string name, T minValue, Func<T> currentValueFunc, TimeSpan cacheDuration)
            where T : IComparable<T>
        {
            Guard.ArgumentNotNull(nameof(builder), builder);
            Guard.ArgumentNotNullOrEmpty(nameof(name), name);
            Guard.ArgumentNotNull(nameof(currentValueFunc), currentValueFunc);

            builder.AddCheck(name, () =>
            {
                var currentValue = currentValueFunc();
                var status = currentValue.CompareTo(minValue) >= 0 ? CheckStatus.Healthy : CheckStatus.Unhealthy;
                return HealthCheckResult.FromStatus(
                    status,
                    $"min={minValue}, current={currentValue}",
                    new Dictionary<string, object> { { "min", minValue }, { "current", currentValue } }
                );
            }, cacheDuration);

            return builder;
        }

        public static HealthCheckBuilder AddMaxValueCheck<T>(this HealthCheckBuilder builder, string name, T maxValue, Func<T> currentValueFunc) where T : IComparable<T>
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            return AddMaxValueCheck(builder, name, maxValue, currentValueFunc, builder.DefaultCacheDuration);
        }

        public static HealthCheckBuilder AddMaxValueCheck<T>(this HealthCheckBuilder builder, string name, T maxValue, Func<T> currentValueFunc, TimeSpan cacheDuration)
            where T : IComparable<T>
        {
            Guard.ArgumentNotNull(nameof(builder), builder);
            Guard.ArgumentNotNullOrEmpty(nameof(name), name);
            Guard.ArgumentNotNull(nameof(currentValueFunc), currentValueFunc);

            builder.AddCheck(name, () =>
            {
                var currentValue = currentValueFunc();
                var status = currentValue.CompareTo(maxValue) <= 0 ? CheckStatus.Healthy : CheckStatus.Unhealthy;
                return HealthCheckResult.FromStatus(
                    status,
                    $"max={maxValue}, current={currentValue}",
                    new Dictionary<string, object> { { "max", maxValue }, { "current", currentValue } }
                );
            }, cacheDuration);

            return builder;
        }

        public static HealthCheckBuilder AddMaxThresholdValueCheck<T>(this HealthCheckBuilder builder, string name, T maxValue, Func<T> currentValueFunc)
            where T : IComparable<T>
        {
            Guard.ArgumentNotNull(nameof(builder), builder);
            Guard.ArgumentNotNullOrEmpty(nameof(name), name);
            Guard.ArgumentNotNull(nameof(currentValueFunc), currentValueFunc);

            builder.AddCheck(name, () =>
            {
                var currentValue = currentValueFunc();
                var status = currentValue.CompareTo(maxValue) <= 0 ? CheckStatus.Healthy : CheckStatus.Unhealthy;
                return HealthCheckResult.FromStatus(
                    status,
                    $"threshold={maxValue}, current={currentValue}",
                    new Dictionary<string, object> { { "threshold", maxValue }, { "current", currentValue } }
                );
            });

            return builder;
        }

        public static HealthCheckBuilder AddMaxThresholdValueCheck<T>(this HealthCheckBuilder builder, string name, T maxValue, Func<T> currentValueFunc, TimeSpan cacheDuration)
            where T : IComparable<T>
        {
            Guard.ArgumentNotNull(nameof(builder), builder);
            Guard.ArgumentNotNullOrEmpty(nameof(name), name);
            Guard.ArgumentNotNull(nameof(currentValueFunc), currentValueFunc);

            builder.AddCheck(name, () =>
            {
                var currentValue = currentValueFunc();
                var status = currentValue.CompareTo(maxValue) <= 0 ? CheckStatus.Healthy : CheckStatus.Unhealthy;
                return HealthCheckResult.FromStatus(
                    status,
                    $"threshold={maxValue}, current={currentValue}",
                    new Dictionary<string, object> { { "threshold", maxValue }, { "current", currentValue } }
                );
            }, cacheDuration);

            return builder;
        }

        public static HealthCheckBuilder AddMaxThresholdValueCheck<T>(this HealthCheckBuilder builder, string name, T maxValue, Func<T> totalValue, Func<T> currentValueFunc1, Func<T> currentValueFunc2, TimeSpan cacheDuration)
            where T : IComparable<T>
        {
            Guard.ArgumentNotNull(nameof(builder), builder);
            Guard.ArgumentNotNullOrEmpty(nameof(name), name);
            Guard.ArgumentNotNull(nameof(currentValueFunc1), currentValueFunc1);
            Guard.ArgumentNotNull(nameof(currentValueFunc2), currentValueFunc2);

            builder.AddCheck(name, () =>
            {
                var total = totalValue();


                var status = total.CompareTo(maxValue) <= 0 ? CheckStatus.Healthy : CheckStatus.Unhealthy;
                return HealthCheckResult.FromStatus(
                    status,
                    $"threshold={maxValue}, totalValue={totalValue}",
                    new Dictionary<string, object> { { "threshold", maxValue }, { "total", totalValue } }
                );
            }, cacheDuration);

            return builder;
        }

        public static HealthCheckBuilder AddMaxThresholdValueCheck<T>(this HealthCheckBuilder builder, string name, T maxValue, Func<T> totalValue, Func<T> currentValueFunc1, Func<T> currentValueFunc2)
            where T : IComparable<T>
        {
            Guard.ArgumentNotNull(nameof(builder), builder);
            Guard.ArgumentNotNullOrEmpty(nameof(name), name);
            Guard.ArgumentNotNull(nameof(currentValueFunc1), currentValueFunc1);
            Guard.ArgumentNotNull(nameof(currentValueFunc2), currentValueFunc2);

            builder.AddCheck(name, () =>
            {
                var total = totalValue();


                var status = total.CompareTo(maxValue) <= 0 ? CheckStatus.Healthy : CheckStatus.Unhealthy;
                return HealthCheckResult.FromStatus(
                    status,
                    $"threshold={maxValue}, totalValue={totalValue}",
                    new Dictionary<string, object> { { "threshold", maxValue }, { "total", totalValue } }
                );
            });

            return builder;
        }
    }
}
