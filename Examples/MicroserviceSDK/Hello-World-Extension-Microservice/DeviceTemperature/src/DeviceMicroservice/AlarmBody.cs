using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceMicroservice
{
    public class AlarmPayload
    {
        public Dictionary<string, string> source { get; set; }      
        public string type { get; set; }
        public string text { get; set; }
        public string severity { get; set; }
        public string status { get; set; }
        public string time { get; set; }       
    }
}
