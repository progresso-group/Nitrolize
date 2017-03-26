using System.Collections.Generic;

namespace Nitrolize.Context
{
    /// <summary>
    /// Interface for a user context.
    /// Can be accessed in fields or before field validation to check for
    /// authentication or authorization or to deliver user specific data.
    /// </summary>
    /// <typeparam name="TId">The type of the user's id.</typeparam>
    public interface IUserContext<TId>
    {
        /// <summary>
        /// Indicates whether the current user is authenticated.
        /// </summary>
        bool IsUserAuthenticated { get; }

        /// <summary>
        /// Gets the id of the current user.
        /// </summary>
        TId UserId { get; }

        /// <summary>
        /// Gets the name of the current user.
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Gets the role names of the current user.
        /// </summary>
        IEnumerable<string> UserRoles { get; }
    }
}
