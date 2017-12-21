using Cumulocity.AspNetCore.Authentication.Basic;
using Cumulocity.SDK.Microservices.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cumulocity.SDK.Microservices.Services
{
    public interface IApplicationService
    {
        Task<IList<User>> GetUsers();
        Task<List<Subscription>> GetCurrentApplicationSubscriptions();

        Task<BasicAuthenticationResult> GetCurrentUser(string authCred);
    }
}
