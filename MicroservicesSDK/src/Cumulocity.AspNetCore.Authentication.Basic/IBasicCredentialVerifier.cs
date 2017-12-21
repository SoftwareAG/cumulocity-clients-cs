using System.Threading.Tasks;

namespace Cumulocity.AspNetCore.Authentication.Basic
{
    /// <summary>
    /// Basic Credentials Verifier
    /// </summary>
    public interface IBasicCredentialVerifier
    {
        /// <summary>
        /// Verifies the credentials received via Basic Authentication
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        Task<BasicAuthenticationResult> Authenticate(string username, string password);
    }
}
