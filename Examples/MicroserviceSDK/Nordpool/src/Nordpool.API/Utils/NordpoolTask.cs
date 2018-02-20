using Cumulocity.SDK.Microservices.Services;
using Cumulocity.SDK.Microservices.Utils.Scheduling;
using Easy.MessageHub;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Nordpool.API.Utils
{
    public class NordpoolTask : IScheduledTask
    {
        public string Schedule => "* */1 * * *";
        private const string nordpoolValues = "nordpool_values";

        public static string NordpoolValues => nordpoolValues;

        public IMemoryCache Cache { get; }
        public IApplicationService Service { get; }
        public MessageHub Hub { get; }

        public NordpoolTask(IMemoryCache cache, IApplicationService service, MessageHub hub)
        {

        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                // HTTP POST
                client.BaseAddress = new Uri("http://www.nordpoolspot.com");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.GetAsync("/api/marketdata/page/35?currency=,,EUR,EUR").Result;
                string res = "";
                using (HttpContent content = response.Content)
                {
                    // ... Read the string.
                    Task<string> result = content.ReadAsStringAsync();
                    res = result.Result;

                    dynamic dynJson = JsonConvert.DeserializeObject(res);

                    foreach (var item in dynJson.data.Rows)
                    {
                        var startTime = Convert.ToDateTime(item.StartTime);
                        var endTime = Convert.ToDateTime(item.EndTime);


                        if ((startTime < DateTime.Now) && (DateTime.Now < endTime))
                        {
                            //Console.WriteLine($"{item.StartTime} {item.EndTime}");
                            foreach (var col in item.Columns)
                            {
                                //Console.WriteLine(col.Name);

                                if (DateTime.Now.Date == DateTime.Now.Date) ;
                            }
                        }
                    }
                }

                return Task.FromResult<object>(null);
        }
    }
}
