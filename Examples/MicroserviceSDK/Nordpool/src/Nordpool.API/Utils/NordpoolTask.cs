using Cumulocity.SDK.Microservices.Services;
using Cumulocity.SDK.Microservices.Settings;
using Cumulocity.SDK.Microservices.Utils.Scheduling;
using Easy.MessageHub;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nordpool.API.Utils
{
    public class NordpoolTask : IScheduledTask
    {
        //One time per min => http://cron.schlitt.info
        public string Schedule => "* */1 * * *";

        public static List<string> devices = new List<string>();

        public string Host { get; set; }
        public string Auth { get; set; } // "username:password1234";

        private const string nordpoolValues = "nordpool_values";

        public static string NordpoolValues => nordpoolValues;

        public IMemoryCache Cache { get; }
        public IApplicationService Service { get; }
        public MessageHub Hub { get; }

        private readonly IHostingEnvironment _hostingEnvironment;

        public NordpoolTask(IHostingEnvironment hostingEnvironment, IMemoryCache cache, IApplicationService service, MessageHub hub, Platform platform)
        {
            _hostingEnvironment = hostingEnvironment;
            Host = @"" + platform.BASEURL;
            Auth = "management/piotr:piotr3333";
        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if ((devices != null) && (devices.Any()))
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
                        CreateMeasurment(price, devices);
                    }
                }
            }

            return Task.FromResult<object>(null);
        }

        private void CreateMeasurment(string price, List<string> devices)
        {
            var measurementTime = DateTime.UtcNow.ToString(("yyyy-MM-ddTHH:mm:ss.fff+02:00"), System.Globalization.CultureInfo.InvariantCulture);
            foreach (var source in devices)
            {
                string jsonBody = @"{
                                ""c8y_energyCost"": {
                                    ""cost"": {
                                        ""value"": " + price + @",
                                        ""unit"":""EUR""
                                    }
                                },
                                ""time"": """ + measurementTime + @""",
                                ""source"": {
                                    ""id"":""" + source + @"""
                                },
                                ""type"":""c8y_energyCost""
                            } ";

                SendPost(jsonBody);
            }
        }

        private void SendPost(string jsonBody)
        {
            var byteArray = Encoding.ASCII.GetBytes(Auth);

            using (var client = new HttpClient())
            {
                StringContent content = AddHeaders(jsonBody, client);

                HttpResponseMessage response = client.PostAsync("measurement/measurements", content).Result;
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("OK");
                }
            }
        }

        private static string ExtractCurrentPrice(dynamic dynJson)
        {
            foreach (var item in dynJson.data.Rows)
            {
                var startTime = DateTime.ParseExact(item.StartTime.ToString(), "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                var endTime = DateTime.ParseExact(item.EndTime.ToString(), "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);

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

        private StringContent AddHeaders(string jsonData, HttpClient client)
        {
            var authenticationHeaderValue = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(
                    System.Text.ASCIIEncoding.ASCII.GetBytes(Auth)
                ));

            client.Timeout = new TimeSpan(0, 0, 30);
            client.BaseAddress = new Uri(Host);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = authenticationHeaderValue;

            StringContent content = new StringContent(jsonData);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            return content;
        }
    }
}