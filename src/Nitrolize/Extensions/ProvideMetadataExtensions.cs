using GraphQL;
using GraphQL.Types;
using System.Collections.Generic;
using System.Linq;

namespace Nitrolize.Extensions
{
    /// <summary>
    /// Extends interface IProvideMetadata by providing support for assigning and checking roles.
    /// </summary>
    public static class ProvideMetadataExtensions
    {
        public static readonly string RolesKey = "Roles";
        public static readonly string AuthenticationKey = "Authentication";
        public static readonly string AuthenticationCheckKey = "AuthenticationCheck";

        /// <summary>
        /// Checks if the IProvideMetadata object requires any roles.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>True, if any role is required, false else.</returns>
        public static bool RequiresRoles(this IProvideMetadata type)
        {
            var roles = type.GetMetadata<IEnumerable<string>>(RolesKey, new List<string>());
            return roles.Any();
        }

        /// <summary>
        /// Checks if the IProvideMetadata object requires any check of user authentication.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool RequiresAuthenticationCheck(this IProvideMetadata type)
        {
            return type.GetMetadata<bool>(AuthenticationCheckKey, false);
        }

        /// <summary>
        /// Checks if the IProviceMetadata object requires authentication.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>True, if authentication is required (default), false else.</returns>
        public static bool RequiresAuthentication(this IProvideMetadata type)
        {
            return type.GetMetadata<bool>(AuthenticationKey, true);
        }

        /// <summary>
        /// Checks if a list of given roles satisfies the requirements.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="userRoles">The roles of a user.</param>
        /// <returns></returns>
        public static bool CanAccess(this IProvideMetadata type, IEnumerable<string> userRoles)
        {
            var requiredRoles = type.GetMetadata<IEnumerable<string>>(RolesKey, new List<string>());
            return requiredRoles.All(x => userRoles?.Contains(x) ?? false);
        }

        /// <summary>
        /// Defines the requirements by providing a list of roles.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="roles">The roles that define the requirements.</param>
        public static void RequiresRoles(this IProvideMetadata type, params string[] roles)
        {
            var requiredRoles = type.GetMetadata<List<string>>(RolesKey);

            if (requiredRoles == null)
            {
                requiredRoles = new List<string>();
                type.Metadata[RolesKey] = requiredRoles;
            }

            if (roles == null)
            {
                return;
            }

            foreach (var role in roles)
            {
                requiredRoles.Fill(role);
            }
        }

        /// <summary>
        /// Defines if authentication is required for this instance of IProvideMetadata.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="isAuthenticationRequired"></param>
        public static void RequiresAuthentication(this IProvideMetadata type, bool isAuthenticationRequired)
        {
            type.Metadata[AuthenticationCheckKey] = true;
            type.Metadata[AuthenticationKey] = isAuthenticationRequired;
        }
    }
}
