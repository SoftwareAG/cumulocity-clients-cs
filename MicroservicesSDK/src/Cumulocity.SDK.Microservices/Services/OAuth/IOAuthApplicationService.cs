using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cumulocity.AspNetCore.Authentication.Basic;
using Cumulocity.SDK.Microservices.Model;
using Cumulocity.SDK.Microservices.OAuth;
using Microsoft.AspNetCore.Http;

namespace Cumulocity.SDK.Microservices.Services.OAuth
{
    public interface IOAuthApplicationService
    {
        Task<IList<User>> GetUsersAsync();
        Task<List<Subscription>> GetCurrentApplicationSubscriptionsAsync();
        Task<OAuthAuthenticationResult> GetCurrentUser(IHeaderDictionary headerDictionary);
    }
}
