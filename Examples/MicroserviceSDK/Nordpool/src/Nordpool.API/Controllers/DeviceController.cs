using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nordpool.API.Utils;

namespace Nordpool.API.Controllers
{
    public class DevicesController : Controller
    {

        // GET api/devices
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // POST api/device
        //[HttpPost]
        //public void Post(int value)
        //{
        //    if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));

        //    NordpoolTask.devices.Add(value.ToString());
        //}

        // POST api/devices
        [HttpPost, Route("api/devices/addDevice")]
        public void  Post([FromBody]string value)
        {
            bool result = Int32.TryParse(value, out int id);
            if (result)
            {
                var cls = User.Claims;
                NordpoolTask.devices.Add(value);
            }

        }
    }
}