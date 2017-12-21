using System;
using System.Collections.Generic;
using System.Text;

namespace Cumulocity.AspNetCore.Authentication.Basic
{
    /// <summary>
    /// Default values used by Basic Access Authentication scheme.
    /// </summary>
    public class BasicAuthenticationDefaults
    {
        /// <summary>
        /// Basic authentication scheme
        /// </summary>
        public const string AuthenticationScheme = "Basic";
        /// <summary>
        /// Default authentication display name
        /// </summary>
        public static readonly string DisplayName = "Basic";
    }
}
