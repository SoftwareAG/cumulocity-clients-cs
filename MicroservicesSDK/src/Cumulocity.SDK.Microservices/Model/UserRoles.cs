using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cumulocity.SDK.Microservices.Model
{
    public class UserRoles
    {
        [JsonProperty(PropertyName = "id")]
        public string UserNo { get; set; }

        [JsonProperty(PropertyName = "lastPasswordChange")]
        public string LastPasswordChange { get; set; }

        [JsonProperty(PropertyName = "self")]
        public string Self { get; set; }

        [JsonProperty(PropertyName = "shouldResetPassword")]
        public string ShouldResetPassword { get; set; }

        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "effectiveRoles")]
        public List<Role> Lists { get; set; }
    }
}
