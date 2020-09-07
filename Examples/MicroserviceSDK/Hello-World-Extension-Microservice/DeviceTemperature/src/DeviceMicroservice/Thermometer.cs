using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceMicroservice
{
    public class Thermometer
    {
        public string nameID { get; set; }

        public override string ToString()
        {
            return "Thermometer Name : " + nameID ;
        }

    }
}
