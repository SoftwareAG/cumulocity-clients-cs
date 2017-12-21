using Cumulocity.SDK.Microservices.Model;
using Cumulocity.SDK.Microservices.Services;
using Cumulocity.SDK.Microservices.Utils.Scheduling;
using Easy.MessageHub;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cumulocity.SDK.Microservices.Utils.Scheduled
{
    public class CurrentApplicationSubscriptionsTask : IScheduledTask
    {
        public string Schedule => "* */1 * * *";
        private const string currentAppSubscriptions = "current_app_subscriptions";
        public IMemoryCache Cache { get; }
        public IApplicationService Service { get; }
        public MessageHub Hub { get; }

        public static string CurrentAppSubscriptions => currentAppSubscriptions;

        public CurrentApplicationSubscriptionsTask(IMemoryCache cache, IApplicationService service, MessageHub hub)
        {
            Cache = cache;
            Service = service;
            Hub = hub;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var subscriptionsNew = await Service.GetCurrentApplicationSubscriptions() ?? new List<Subscription>();
            var subscriptionsOld = Cache.Get<List<Subscription>>(CurrentAppSubscriptions) ?? new List<Subscription>();

            PublishChangedSubscription(subscriptionsNew.Except(subscriptionsOld, Subscription.SubscriptionComparer),
                                       subscriptionsOld.Except(subscriptionsNew, Subscription.SubscriptionComparer));

            Cache.Set(CurrentAppSubscriptions, subscriptionsNew);
        }

        private void PublishChangedSubscription(IEnumerable<Subscription> removed, IEnumerable<Subscription> added)
        {
            List<ChangedSubscription> result = new List<ChangedSubscription>();
            result.AddRange(removed.Select(r=>new ChangedSubscription { Name = r.Name, Password = r.Password, Status = "D", Tenant = r.Tenant }));
            result.AddRange(added.Select(r => new ChangedSubscription { Name = r.Name, Password = r.Password, Status = "A", Tenant = r.Tenant }));
            Hub.Publish(result);
        }
    }
}