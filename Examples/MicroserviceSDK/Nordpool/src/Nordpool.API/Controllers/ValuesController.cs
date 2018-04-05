using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Nordpool.API.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {

        private readonly IHostingEnvironment _hostingEnvironment;
        public ValuesController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value01", "value02" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        [HttpGet()]
        [Route("~/api/getlog/")]
        public string GetLog()
        {
            return ReadFile();
        }
        public string ReadFile()
        {
            try
            {
                //string contentRootPath = _hostingEnvironment.ContentRootPath;
                //var logFile = Path.Combine(contentRootPath, "Logs\\myapp-20180329.txt");

                //using (var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                //using (StreamReader reader = new StreamReader(fs, Encoding.Default))
                //{
                //    string fileContent = reader.ReadToEnd();
                //    if (fileContent != null && fileContent != "")
                //    {
                //        return fileContent;
                //    }
                //}
            }
            catch (Exception ex)
            {
                //Log
                throw ex;
            }
            return null;
        }
    }
}
