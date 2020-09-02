using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceMicroservice
{
    public class Thermometer
    {
        public string nameID { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public override string ToString()
        {
            return "Thermometer Name : " + nameID + "\nTemperatureC: " + TemperatureC + "\nTemperatureF: " + TemperatureF;
        }

    }
}
