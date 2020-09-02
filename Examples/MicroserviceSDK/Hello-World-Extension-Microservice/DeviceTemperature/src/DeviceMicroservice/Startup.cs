
namespace DeviceMicroservice
{


	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.DependencyInjection.Extensions;
	using Microsoft.Extensions.Logging;
	using Microsoft.Extensions.Options;
	using Newtonsoft.Json.Serialization;
    using Cumulocity.SDK.Microservices.BasicAuthentication;
    using Cumulocity.SDK.Microservices.Configure;
    using Cumulocity.SDK.Microservices.Services;
    using Cumulocity.SDK.Microservices.Settings;
    using Cumulocity.SDK.Microservices.Utils;
    using Microsoft.AspNetCore.Builder;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMemoryCache();
			services.AddCumulocityAuthentication(Configuration);
			services.AddPlatform(Configuration);
			services.AddSingleton<IApplicationService, ApplicationService>();
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			//MVC
			services.AddControllers(options => options.EnableEndpointRouting = false);
			services.Replace(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(TimedLogger<>)));
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
		{
			app.UseAuthentication();
			app.UseBasicAuthentication();
			app.UseMvcWithDefaultRoute();
		}
	}
}
