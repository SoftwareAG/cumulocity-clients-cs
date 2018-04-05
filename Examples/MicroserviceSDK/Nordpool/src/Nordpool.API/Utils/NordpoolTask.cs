using Cumulocity.SDK.Microservices.Services;
using Cumulocity.SDK.Microservices.Settings;
using Cumulocity.SDK.Microservices.Utils.Scheduling;
using Easy.MessageHub;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cumulocity.SDK.Microservices.Model;
using Cumulocity.SDK.Microservices.Utils.Extenions;

namespace Nordpool.API.Utils
{
    public class NordpoolTask : IScheduledTask
    {
        //One time per min => http://cron.schlitt.info
        public string Schedule => "* */1 * * *";

        public static List<string> devices = new List<string>();
        public string Host { get; set; }
        public string Auth { get; set; }

        private const string nordpoolValues = "nordpool_values";
        public static string NordpoolValues => nordpoolValues;
        public IMemoryCache Cache { get; }
        public Platform Platform { get; }
        public Guid SubscriptionToken { get; }

        public NordpoolTask(IMemoryCache cache, IApplicationService service, MessageHub hub, Platform platform)
        {
            SubscriptionToken = hub.Subscribe<List<ChangedSubscription>>(OnChangedSubscription);
            Cache = cache;
            Platform = platform;
            Host = @"" + platform.BASEURL;
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

                    using (HttpContent content = response.Content)
                    {
                        Task<string> result = content.ReadAsStringAsync();
                        dynamic dynJson = JsonConvert.DeserializeObject(result.Result);

                        string price = ExtractCurrentPrice(dynJson);
  
                        CreateMeasurment(price, devices);
                    }
                }
            }

            return Task.FromResult<object>(null);
        }

        private void CreateMeasurment(string price, List<string> devices)
        {
            try
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
            catch (Exception ex)
            {
            }
        }

        private void SendPost(string jsonBody)
        {
            var byteArray = Encoding.ASCII.GetBytes(Auth);

            var subs = Cache.Get<List<Subscription>>("current_app_subscriptions");

            if (subs != null)
            {
                foreach (var sub in subs)
                {
                    using (var client = new HttpClient())
                    {
                        StringContent content = AddHeaders(jsonBody, client, sub);

                        HttpResponseMessage response = client.PostAsync("measurement/measurements", content).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            Debug.WriteLine("SuccessStatusCode");
                        }

                    }
                }
            }
        }

        private string ExtractCurrentPrice(dynamic dynJson)
        {

            foreach (var item in dynJson.data.Rows)
            {
                try
                {

                    var startTime = DateTime.ParseExact(item.StartTime.ToString(), "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    var endTime = DateTime.ParseExact(item.EndTime.ToString(), "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                    var v1 = TimeSpan.Compare(startTime.TimeOfDay, DateTime.Now.TimeOfDay) <= 0;
                    var v2 = TimeSpan.Compare(DateTime.Now.TimeOfDay, endTime.TimeOfDay) <= 0;


                    if (v1 && v2)
                    {
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
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            return null;
        }

        private StringContent AddHeaders(string jsonBody, HttpClient client, Subscription sub)
        {
            string authCred = GetAuthCredentialsBase64(
                sub.Tenant,
                sub.Name,
                sub.Password);

            var authenticationHeaderValue =
                 new AuthenticationHeaderValue("Basic", authCred);

            client.Timeout = new TimeSpan(0, 0, 30);
            client.BaseAddress = new Uri(Host);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = authenticationHeaderValue;

            StringContent content = new StringContent(jsonBody);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            return content;
        }

        private static string GetAuthCredentialsBase64(string tenant, string username, string password)
        {
            return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(tenant + "/" + username + ":" + password));
        }

        private void OnChangedSubscription(List<ChangedSubscription> lstChangedSubscriptions)
        {
            foreach (var subscription in lstChangedSubscriptions)
            {
                
            }
        }
    }
}