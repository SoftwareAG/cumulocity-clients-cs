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
                    Task<string> result = content.ReadAsStringAsync();
                    res = result.Result;

                    dynamic dynJson = JsonConvert.DeserializeObject(res);

                    string price = ExtractCurrentPrice(dynJson);
                    List<string> devices = new List<string>();
                    CreateMeasurment(price,devices);
                }
            }

                return Task.FromResult<object>(null);
        }

        private void CreateMeasurment(string price, List<string> devices)
        {
            var measurementTime = DateTime.UtcNow.ToString(("yyyy-MM-ddTHH:mm:ss.fff+02:00"), System.Globalization.CultureInfo.InvariantCulture);

            string jsonBody = @"{
                        'energy': {
                            'cost': {
                                'value': " + price + @",
                        'unit': 'EUR' }
                                },
                    'time':" + measurementTime + @",
                    'source': {
                                    'id': source },
                    'type': 'energyCost'
                  }";

            //Here: Send Post
        }

        private static string ExtractCurrentPrice(dynamic dynJson)
        {
            foreach (var item in dynJson.data.Rows)
            {
                var startTime = Convert.ToDateTime(item.StartTime);
                var endTime = Convert.ToDateTime(item.EndTime);


                var v1 = TimeSpan.Compare(startTime.TimeOfDay, DateTime.Now.TimeOfDay) <= 0;
                var v2 = TimeSpan.Compare(DateTime.Now.TimeOfDay, endTime.TimeOfDay) <= 0;

                if (v1 && v2)
                {
                    //Console.WriteLine($"{item.StartTime} {item.EndTime}");
                    foreach (var col in item.Columns)
                    {
                        if (DateTime.Now.Date == DateTime.Now.Date)
                        {
                            string value = Convert.ToString(col.Value);
                            return value.Replace(',', '.');
                        }
                    }
                }
            }

            return null;
        }
    }
}
