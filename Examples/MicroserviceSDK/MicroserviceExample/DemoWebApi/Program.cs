using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DemoWebApi
{
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
					var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
					var port = Environment.GetEnvironmentVariable("SERVER_PORT");

					int portNumber = 8080;

					if (Int32.TryParse(port, out portNumber))
					{
						options.Listen(IPAddress.Parse("0.0.0.0"), portNumber);
					}
					else
					{
						options.Listen(IPAddress.Parse("0.0.0.0"), 8080);
					}
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
