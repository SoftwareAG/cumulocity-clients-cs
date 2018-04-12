using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cumulocity.AspNetCore.Authentication.Basic;
using Cumulocity.SDK.Microservices.HealthCheck.Extentions;
using Cumulocity.SDK.Microservices.HealthCheck.Extentions.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json;

namespace Cumulocity.SDK.Microservices.HealthCheck.AspNetCore
{
    public class HealthCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _path;
        private readonly int? _port;
        private readonly IHealthCheckService _service;
        private readonly TimeSpan _timeout;

        public HealthCheckMiddleware(RequestDelegate next, IHealthCheckService service, int port, TimeSpan timeout)
        {
            _port = port;
            _service = service;
            _next = next;
            _timeout = timeout;
        }

        public HealthCheckMiddleware(RequestDelegate next, IHealthCheckService service, string path, TimeSpan timeout)
        {
            _path = path;
            _service = service;
            _next = next;
            _timeout = timeout;
        }

        public async Task Invoke(HttpContext context)
        {
            if (IsHealthCheckRequest(context))
            {
                var isAuthenticated = await context.AuthenticateAsync(BasicAuthenticationDefaults.AuthenticationScheme);
                var timeoutTokenSource = new CancellationTokenSource(_timeout);
                var result = await _service.CheckHealthAsync(timeoutTokenSource.Token);
                var status = result.Status;

                var hcResultRootObject = new RootObject() { status = result.Status.ToString(), details = new Dictionary<string, Health>() };

                if (isAuthenticated.Succeeded)
                {
                    foreach (var res in result.Results)
                    {
                        var newItem = new Health()
                        {
                            status = res.Value.Status.ToString()
                        };

                        if (res.Value.Data.Count > 0)
                        {
                            newItem.details = res.Value.Data.ToDictionary(v => v.Key, v => v.Value);
                        }
                        else
                        {
                            newItem.details = new Dictionary<string, object>();
                        }

                        hcResultRootObject.details.Add(res.Key, newItem);
                    }
                }


                if (status != CheckStatus.Healthy)
                    context.Response.StatusCode = 503;

                context.Response.Headers.Add("content-type", "application/json");
                await context.Response.WriteAsync(JsonConvert.SerializeObject(hcResultRootObject));
                //await context.Response.WriteAsync(JsonConvert.SerializeObject(new { status = status.ToString() }));
                return;
            }
            else
            {
                await _next.Invoke(context);
            }
        }

        private bool IsHealthCheckRequest(HttpContext context)
        {
            if (_port.HasValue)
            {
                var connInfo = context.Features.Get<IHttpConnectionFeature>();
                if (connInfo.LocalPort == _port)
                    return true;
            }

            if (context.Request.Path == _path)
            {
                return true;
            }

            return false;
        }
    }
}
