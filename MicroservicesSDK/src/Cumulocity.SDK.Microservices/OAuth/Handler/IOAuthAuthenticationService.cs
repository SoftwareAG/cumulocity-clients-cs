using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Cumulocity.SDK.Microservices.OAuth.Handler
{
    public interface IOAuthAuthenticationService
    {
        Task<bool> IsValidUserAsync(OAuthAuthenticationOptions options, IHeaderDictionary Headers);
    }
}
