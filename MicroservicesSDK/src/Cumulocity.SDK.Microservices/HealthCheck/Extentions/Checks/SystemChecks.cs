using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Cumulocity.SDK.Microservices.HealthCheck.Extentions.Checks
{
    public static partial class HealthCheckBuilderExtensions
    {
        // System checks

        public static HealthCheckBuilder AddPrivateMemorySizeCheck(this HealthCheckBuilder builder, long maxSize)
            => AddMaxValueCheck(builder, $"PrivateMemorySize({maxSize})", maxSize, () => Process.GetCurrentProcess().PrivateMemorySize64);

        public static HealthCheckBuilder AddPrivateMemorySizeCheck(this HealthCheckBuilder builder, long maxSize, TimeSpan cacheDuration)
            => AddMaxValueCheck(builder, $"PrivateMemorySize({maxSize})", maxSize, () => Process.GetCurrentProcess().PrivateMemorySize64, cacheDuration);

        public static HealthCheckBuilder AddVirtualMemorySizeCheck(this HealthCheckBuilder builder, long maxSize)
            => AddMaxValueCheck(builder, $"VirtualMemorySize({maxSize})", maxSize, () => Process.GetCurrentProcess().VirtualMemorySize64);

        public static HealthCheckBuilder AddVirtualMemorySizeCheck(this HealthCheckBuilder builder, long maxSize, TimeSpan cacheDuration)
            => AddMaxValueCheck(builder, $"VirtualMemorySize({maxSize})", maxSize, () => Process.GetCurrentProcess().VirtualMemorySize64, cacheDuration);

        public static HealthCheckBuilder AddWorkingSetCheck(this HealthCheckBuilder builder, long maxSize)
            => AddMaxValueCheck(builder, $"WorkingSet({maxSize})", maxSize, () => Process.GetCurrentProcess().WorkingSet64);

        public static HealthCheckBuilder AddWorkingSetCheck(this HealthCheckBuilder builder, long maxSize, TimeSpan cacheDuration)
            => AddMaxValueCheck(builder, $"WorkingSet({maxSize})", maxSize, () => Process.GetCurrentProcess().WorkingSet64, cacheDuration);

        public static HealthCheckBuilder AddDiskSpaceCheck(this HealthCheckBuilder builder, long maxSize, TimeSpan cacheDuration)
            => AddMaxThresholdValueCheck(builder, $"diskSpace", maxSize, () => Process.GetCurrentProcess().WorkingSet64, cacheDuration);

        public static HealthCheckBuilder AddDiskSpaceCheck(this HealthCheckBuilder builder, long maxSize)
            => AddMaxThresholdValueCheck(builder, $"diskSpace", maxSize, () => Process.GetCurrentProcess().WorkingSet64);

        public static HealthCheckBuilder AddMemoryUsageCheck(this HealthCheckBuilder builder, long maxSize, TimeSpan cacheDuration)
            => AddMaxThresholdValueCheck(builder, $"memoryUsage", maxSize, () => Process.GetCurrentProcess().PrivateMemorySize64 + Process.GetCurrentProcess().VirtualMemorySize64, () => Process.GetCurrentProcess().PrivateMemorySize64, () => Process.GetCurrentProcess().VirtualMemorySize64, cacheDuration);


        public static HealthCheckBuilder AddMemoryUsageCheck(this HealthCheckBuilder builder, long maxSize)
            => AddMaxThresholdValueCheck(builder, $"memoryUsage", maxSize, () => Process.GetCurrentProcess().PrivateMemorySize64 + Process.GetCurrentProcess().VirtualMemorySize64, () => Process.GetCurrentProcess().PrivateMemorySize64, () => Process.GetCurrentProcess().VirtualMemorySize64);
    }
}
