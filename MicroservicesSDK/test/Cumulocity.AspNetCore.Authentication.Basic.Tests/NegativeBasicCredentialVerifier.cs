using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cumulocity.AspNetCore.Authentication.Basic.Tests
{
    internal class NegativeBasicCredentialVerifier : IBasicCredentialVerifier
    {
        Task<BasicAuthenticationResult> IBasicCredentialVerifier.Authenticate(string username, string password) =>
                                                                   Task.FromResult(new BasicAuthenticationResult());
    }
}
