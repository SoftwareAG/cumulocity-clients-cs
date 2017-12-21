using System;
using System.Threading.Tasks;

namespace Cumulocity.AspNetCore.Authentication.Basic
{
    public class FuncBasicCredentialVerifier : IBasicCredentialVerifier
    {
        private readonly Func<(string username, string password), Task<BasicAuthenticationResult>> _func;

        public FuncBasicCredentialVerifier(Func<(string username, string password), Task<BasicAuthenticationResult>> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        Task<BasicAuthenticationResult> IBasicCredentialVerifier.Authenticate(string username, string password)
        {
            return _func((username, password));
        }
    }
}
