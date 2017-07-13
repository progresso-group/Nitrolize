using Nitrolize.Context;
using System.Collections.Generic;

namespace Nitrolize.Tests.Integration.Authentication
{
    public class MockUserContext : IUserContext<int>
    {
        public bool IsUserAuthenticated
        {
            get
            {
                return true;
            }
        }

        public int UserId
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the name of the current user.
        /// </summary>
        public string UserName
        {
            get
            {
                return "fake user";
            }
        }

        /// <summary>
        /// Gets the roles of the current user.
        /// </summary>
        public IEnumerable<string> UserRoles
        {
            get
            {
                return new List<string>();
            }
        }
    }
}
