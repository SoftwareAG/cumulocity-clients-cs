using Cumulocity.AspNetCore.Authentication.Basic.Events;
using System;
using System.Threading.Tasks;

namespace Cumulocity.AspNetCore.Authentication.Basic
{
    /// <summary>
    /// Basic authentication events
    /// </summary>
    public class BasicAuthenticationEvents
    {
        /// <summary>
        /// Invoked if exceptions are thrown during request processing. The exceptions will be re-thrown after this event unless suppressed.
        /// </summary>
        public Func<AuthenticationFailedContext, Task> OnAuthenticationFailed { get; set; } = context => Task.CompletedTask;

        /// <summary>
        /// Invoked after the security token has passed validation and a ClaimsIdentity has been generated.
        /// </summary>
        public Func<AuthenticationSucceededContext, Task> OnCredentialsValidated { get; set; } = context => Task.CompletedTask;

        /// <summary>
        /// Invoked before a challenge is sent back to the caller.
        /// </summary>
        public Func<BasicAuthenticationChallengeContext, Task> OnChallenge { get; set; } = context => Task.CompletedTask;

        public virtual Task AuthenticationFailed(AuthenticationFailedContext context) => OnAuthenticationFailed(context);

        public virtual Task CredentialsValidated(AuthenticationSucceededContext context) => OnCredentialsValidated(context);

        public virtual Task Challenge(BasicAuthenticationChallengeContext context) => OnChallenge(context);
    }
}
