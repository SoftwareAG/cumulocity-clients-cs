using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Cumulocity.SDK.Microservices.Settings;
using Easy.MessageHub;
using System;
using Cumulocity.SDK.Microservices.Model;
using System.Collections.Generic;

namespace DockerWebApp.Demo.Controllers
{
    [Authorize(Roles = "ROLE_APPLICATION_MANAGEMENT_READ")]
    public class HomeController : Controller
    {
        private readonly MessageHub _hub;
        private readonly Guid _subscriptionToken;

        public HomeController(Platform platform, MessageHub hub)
        {
            _hub = hub;
            _subscriptionToken = _hub.Subscribe<List<ChangedSubscription>>(OnChangedSubscription);
        }

        public IActionResult Index()
        {
            return View();
        }

        private void OnChangedSubscription(List<ChangedSubscription> obj)
        {

        }
    }
}