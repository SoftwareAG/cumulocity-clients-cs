using Cumulocity.SDK.Microservices.Configure;
using Cumulocity.SDK.Microservices.Services;
using Cumulocity.SDK.Microservices.Settings;
using Cumulocity.SDK.Microservices.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;

namespace DemoOAuth
{
    public class Startup
    {
        private readonly ILogger _logger;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            _logger = logger;
            CookieScheme = "YourSchemeName";
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddPlatform(Configuration);
            services.AddSingleton<IApplicationService, ApplicationService>();
            services.Replace(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(TimedLogger<>)));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddControllers(options => options.EnableEndpointRouting = false);
            services.AddMemoryCache();
            services.AddCumulocityAuthenticationAll(Configuration);
        }

        public static string CookieScheme { get; private set; }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
        }
    }
}
