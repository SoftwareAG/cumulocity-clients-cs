using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cumulocity.SDK.Microservices.Model;
using Cumulocity.SDK.Microservices.Settings;
using Easy.MessageHub;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DemoWebApi.Controllers
{
	[Route("api/[controller]")]
	public class DataController : Controller
    {
	    private Guid SubscriptionToken { get; }
		public IMemoryCache Cache { get; }

		public DataController(IMemoryCache cache, Platform platform, MessageHub hub)
		{
			SubscriptionToken = hub.Subscribe<List<ChangedSubscription>>(OnChangedSubscription);
			Cache = cache;
		}

	    // GET api/data
	    [HttpGet]
	    public IEnumerable<string> Get()
		{
			
			return new string[] { "d1", "d2" };
	    }

		// GET api/data/subscriptions
		[HttpGet("subscriptions")]
		public IEnumerable<Subscription> GetSubscription()
	    {
		    var subs = Cache.Get<List<Subscription>>("current_app_subscriptions");
		    return subs;
	    }

		private void OnChangedSubscription(List<ChangedSubscription> obj)
	    {

	    }
	}
}