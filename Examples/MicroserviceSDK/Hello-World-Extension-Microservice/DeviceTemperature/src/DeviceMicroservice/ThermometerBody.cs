using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceMicroservice
{
    public class ThermometerBody
    {
        public string name { get; set; }
        public Dictionary<string, string> c8y_IsDevice { get; set; }
        public Dictionary<string, string> c8y_IsThermometer { get; set; }
        public string[] c8y_SupportedMeasurements { get; set; }

    }

}
