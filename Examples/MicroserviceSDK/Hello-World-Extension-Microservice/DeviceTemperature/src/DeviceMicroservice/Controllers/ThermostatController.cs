using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Cumulocity.SDK.Microservices.Settings;
using RestSharp;
using DeviceMicroservice;
using DeviceMicroservice.Controllers;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ThermostatMicroservice.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ThermostatController : ControllerBase
    {
        public Platform PlatformSettings { get; }
        private readonly ILogger<WeatherForecastController> _logger;

        public ThermostatController(Platform platform, ILogger<WeatherForecastController> logger)
        {
            PlatformSettings = platform;
            _logger = logger;
        }

        // This endpoint is to retrive information about all the thermostat thermometers.
        // GET Thermostat/thermometers
        [HttpGet("thermometers")]
        public async Task<ActionResult<string>> Get()
        {
            var client = new RestClient(PlatformSettings.BASEURL + "/inventory/managedObjects?query =$filter = (has(c8y_IsThermometer) + and + has(c8y_SupportedMeasurements)");
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", Request.Headers["Authorization"]);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            IRestResponse response = client.Execute(request);
            Console.WriteLine("The thermometers in the inventory are returned.");
            return response.Content;
        }

        // This endpoint creates thermometer (temprature measurment supported device in cumulocity) as per the JSON body of the object and sends one temprature measurment which is there in the body.
        // POST Thermostat/thermometers
        [HttpPost("thermometers")]
        public Thermometer Post([FromBody] Thermometer thermometer)
        {
            try
            {
                if (thermometer == null)
                {
                    Console.WriteLine("Add temprature object json in the request body.");
                    return null;
                }

                string thermometerCreationPayload = JsonSerializer.Serialize(new ThermometerBody
                {
                    name = thermometer.nameID,
                    c8y_IsDevice = new Dictionary<string, string> { },
                    c8y_IsThermometer = new Dictionary<string, string> { },
                    c8y_SupportedMeasurements = new string[] { "c8y_TemperatureMeasurement" }
                });

                var client = new RestClient(PlatformSettings.BASEURL+"/inventory/managedObjects");
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", Request.Headers["Authorization"]);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Accept", "application/json");
                request.AddParameter("application/json", thermometerCreationPayload, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                Console.WriteLine((response.StatusCode.ToString().Equals("Created")) ? "Thermometer created !!" : "Thermometer couldn't be created !! \n Below is the response body from cumulocity \n");
                Console.WriteLine(response.Content);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return thermometer;
        }

        // We are calling the default weather forcast endpoint and if any returned forecast temprature is greater than 25º then we are raising an alarm with alarmText 
        // POST thermometers/<DeviceID>/temperatures
        [HttpPost("thermometers/{id}/temperatures")]
        public string Post([FromBody]AlarmText alarmText)
        {
            try
            {
                //Mock of hitting endpoint <microservice-name>/weatherforecast to take the latest entry and post the temperatureC value to Cumulocity IoT under the specified device(by id).
                WeatherForecastController w = new WeatherForecastController(_logger);
                var forecastList = w.Get();
                var id = Request.RouteValues.Values.ElementAt(0); // To retrive the device ID from the Request URL , which will be later used to create an alarm.

                string alarmCreationPayload = JsonSerializer.Serialize(new AlarmPayload
                {
                    source = new Dictionary<string, string> { { "id", (string)id } },
                    type = "c8y_hightemperature_alarm",
                    text = alarmText.alarmText,
                    severity = "WARNING",
                    status = "ACTIVE",
                    time = "2020-03-03T12:03:27.845Z"
                });

                // If any of the forecast is higher than 25º then we raise an alarm for the device ID passed in the request.
                foreach (var forecast in forecastList)
                {
                    if (forecast.TemperatureC > 25)
                    {
                        Console.Write(forecast.TemperatureC + "º temprature was recorded and this is considerded as " + forecast.Summary + " temprature. \nWe are raising an alarm on object with ID "+ id + " in the platform.");
                        var client = new RestClient(PlatformSettings.BASEURL+"/alarm/alarms");
                        var request = new RestRequest(Method.POST);
                        request.AddHeader("Authorization", Request.Headers["Authorization"]);
                        request.AddHeader("Content-Type", "application/vnd.com.nsn.cumulocity.alarm+json");
                        request.AddHeader("Accept", "application/vnd.com.nsn.cumulocity.alarm+json");
                        request.AddParameter("application/vnd.com.nsn.cumulocity.alarm+json", alarmCreationPayload , ParameterType.RequestBody);
                        IRestResponse response = client.Execute(request);
                        Console.WriteLine(response.Content);
                        return response.Content.ToString();
                    }
                }
             
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return "No High temprature was observed in the forecast. So no alarms were raised. Please hit this endpoint till high temprature is observed.";
        }

    }
}