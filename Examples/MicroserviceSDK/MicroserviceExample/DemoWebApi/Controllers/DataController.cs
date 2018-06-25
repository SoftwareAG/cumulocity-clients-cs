using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cumulocity.AspNetCore.Authentication.Basic;
using Cumulocity.SDK.Microservices.Model;
using Cumulocity.SDK.Microservices.Services;
using Cumulocity.SDK.Microservices.Settings;
using DemoWebApi.Helpers;
using Easy.MessageHub;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DemoWebApi.Controllers
{
	[Route("api/[controller]")]
	public class DataController : Controller
    {
	    private Guid SubscriptionToken { get; }
		public IMemoryCache Cache { get; }
		public Platform PlatformSettings { get; }
	    public IApplicationService Service { get; }

	    public DataController(IMemoryCache cache, Platform platform, MessageHub hub, IApplicationService service)
		{
			SubscriptionToken = hub.Subscribe<List<ChangedSubscription>>(OnChangedSubscription);
			Cache = cache;
			PlatformSettings = platform;
			Service = service;
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

		// GET api/data/currentuser
		[HttpGet("currentuser")]
	    public async Task<BasicAuthenticationResult> GetCurrentUser()
		{
			return await Service.GetCurrentUser("authCred");
	    }

		// GET api/data/user
		[HttpGet("user")]
	    public async Task<IList<User>> GetUser()
		{
			return await Service.GetUsers();
		}

		// GET api/data/permissions
		[HttpGet("permissions")]
		[Authorize(Roles = "ROLE_APPLICATION_MANAGEMENT_READ")]
		public IEnumerable<string> CheckPermissions()
	    {
		    return new string[] { "permission1", "permission2" };
	    }

		// GET api/data/otherpermission
		[HttpGet("otherpermission")]
	    [Authorize(Roles = "ROLE_APPLICATION_MANAGEMENT_READ2")]
	    public IEnumerable<string> CheckOtherPermissions()
	    {
		    return new string[] {"otherpermission1", "otherpermission2"};
	    }

	    [HttpGet("scheduledtask")]
		public IEnumerable<int> CheckTimer()
	    {
		    for (int i = 0; i < 2; ++i)
		    {
			    Task.WaitAll(Task.Delay(1000));
		    }

		    return new int[] { TimerCounter.Counter };
		}

	    [HttpGet("platform")]
		[ProducesResponseType(200, Type = typeof(Platform))]
	    [ProducesResponseType(404)]
	    public IActionResult GetPlatform()
		{
			return Ok(PlatformSettings);
	    }


		private void OnChangedSubscription(List<ChangedSubscription> obj)
	    {

	    }
	}
}