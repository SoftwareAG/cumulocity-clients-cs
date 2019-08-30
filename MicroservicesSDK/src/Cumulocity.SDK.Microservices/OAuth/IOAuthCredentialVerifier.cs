using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cumulocity.SDK.Microservices.OAuth.Handler;
using Microsoft.AspNetCore.Http;

namespace Cumulocity.SDK.Microservices.OAuth
{
    public interface IOAuthCredentialVerifier
    {
        Task<OAuthAuthenticationResult> AuthenticateAsync(OAuthAuthenticationOptions options, IHeaderDictionary headers);
    }
}
