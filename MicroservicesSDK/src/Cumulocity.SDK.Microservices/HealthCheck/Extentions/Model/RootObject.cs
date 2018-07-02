using System;
using System.Collections.Generic;
using System.Text;

namespace Cumulocity.SDK.Microservices.HealthCheck.Extentions.Model
{
    public class RootObject
    {
        public string status { get; set; }
        public Dictionary<String, Health> details { get; set; }
    }

    public class Health
    {
        public string status { get; set; }
        public Dictionary<String, object> details { get; set; }
    }
}
