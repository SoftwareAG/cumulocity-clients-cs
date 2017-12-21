using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cumulocity.SDK.Microservices.Model
{
    public class Role
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "self")]
        public string Self { get; set; }
    }
}
