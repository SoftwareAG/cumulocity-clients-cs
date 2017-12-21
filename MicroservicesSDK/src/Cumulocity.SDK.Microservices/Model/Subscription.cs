using Newtonsoft.Json;
using System.Collections.Generic;

namespace Cumulocity.SDK.Microservices.Model
{
    public class Subscription
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string Tenant { get; set; }

        private sealed class SubscriptionEqualityComparer : IEqualityComparer<Subscription>
        {
            public bool Equals(Subscription x, Subscription y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Name == y.Name && x.Password == y.Password && x.Tenant == y.Tenant;
            }

            public int GetHashCode(Subscription obj)
            {
                unchecked
                {
                    var hashCode = 13;
                    var nameHashCode = !string.IsNullOrEmpty(obj.Name) ? obj.Name.GetHashCode() : 0;
                    var passwordHashCode = !string.IsNullOrEmpty(obj.Name) ? obj.Name.GetHashCode() : 0;
                    var tenantHashCode = !string.IsNullOrEmpty(obj.Name) ? obj.Name.GetHashCode() : 0;

                    hashCode = (hashCode * 397) ^ nameHashCode;
                    hashCode = (hashCode * 397) ^ passwordHashCode;
                    hashCode = (hashCode * 397) ^ tenantHashCode;

                    return hashCode;
                }
            }
        }

        private static readonly IEqualityComparer<Subscription> SubscriptionComparerInstance
                        = new SubscriptionEqualityComparer();

        public static IEqualityComparer<Subscription> SubscriptionComparer
        {
            get { return SubscriptionComparerInstance; }
        }
    }

    public class ChangedSubscription : Subscription
    {
        public string Status { get; set; }
    }

    public class CurrentApplicationSubscription
    {
        [JsonProperty("users")]
        public List<Subscription> Users { get; set; }
    }
}