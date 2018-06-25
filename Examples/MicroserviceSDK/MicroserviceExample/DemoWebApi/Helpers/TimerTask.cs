using Cumulocity.SDK.Microservices.Model;
using Cumulocity.SDK.Microservices.Services;
using Cumulocity.SDK.Microservices.Settings;
using Cumulocity.SDK.Microservices.Utils.Scheduling;
using DemoWebApi.Helpers;
using Easy.MessageHub;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DemoWebApi
{
	internal class TimerTask : IScheduledTask
	{
		//One time per min => http://cron.schlitt.info
		public string Schedule => "* */1 * * *";

		public Guid SubscriptionToken { get; }

		private readonly IMemoryCache Cache;

		public Platform Platform { get; }

		private readonly int Host;
		private readonly Action<List<ChangedSubscription>> OnChangedSubscription;

		//public TimerTask(IMemoryCache cache, IApplicationService service, MessageHub hub, Platform platform)
		//{
		//	SubscriptionToken = hub.Subscribe<List<ChangedSubscription>>(OnChangedSubscription);
		//	Cache = cache;
		//	Platform = platform;
		//}

		public Task ExecuteAsync(CancellationToken cancellationToken)
		{
			if (TimerCounter.Counter >= int.MaxValue - 1)
			{
				TimerCounter.Counter = 3;
			}
			TimerCounter.Counter += 1;
			return Task.FromResult<object>(null);
		}
	}
}