using System;

namespace Cumulocity.SDK.Microservices.IntegrationTest.Utils
{
	public static class Guard
	{
		public static void ThrowIfNull(object argumentValue, string argumentName)
		{
			if (argumentValue == null)
			{
				throw new ArgumentNullException(argumentName);
			}
		}
	}
}