using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeviceMicroservice
{

  
using System.Net;
using Cumulocity.SDK.Microservices.Configure;
using Microsoft.AspNetCore;

   public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

		public static IWebHost BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseKestrel(options =>
				{
					var port = Environment.GetEnvironmentVariable("SERVER_PORT");
					options.Listen(IPAddress.Parse("0.0.0.0"), Int32.TryParse(port, out var portNumber) ? portNumber : 8080);
				})
				.ConfigureLogging((hostingContext, logging) =>
				{
					logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
					logging.AddConsole().SetMinimumLevel(LogLevel.Information);
				})
				.UseStartup<Startup>()
				.Build();
    }
}
