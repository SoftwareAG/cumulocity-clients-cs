using Cumulocity.AspNetCore.Authentication.Basic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cumulocity.SDK.Microservices.BasicAuthentication
{
    public class BasicAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;


        public BasicAuthenticationMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<BasicAuthenticationMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            _logger.LogInformation("Handling request: " + context.Request.Path);
            var result = await context.AuthenticateAsync(BasicAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                await context.ChallengeAsync(BasicAuthenticationDefaults.AuthenticationScheme);
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BasicAuthenticationMiddleware>();
        }
    }
}
