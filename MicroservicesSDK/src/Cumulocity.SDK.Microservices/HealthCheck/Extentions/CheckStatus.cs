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
	public enum HealthyStatus
	{
		UP,
		DOWN
	}

	public static class CheckStatusExtensions
	{
		public static HealthyStatus ToHealthyStatus(this CheckStatus value)
		{
			switch (value)
			{
				case CheckStatus.Healthy:
					return HealthyStatus.UP;
				default:
					return HealthyStatus.DOWN;
			}
		}
	}
}
