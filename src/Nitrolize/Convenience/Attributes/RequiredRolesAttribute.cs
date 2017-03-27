using System;

namespace Nitrolize.Convenience.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredRolesAttribute : Attribute
    {
        public RequiredRolesAttribute(params string[] requiredRoles)
        {
            this.RequiredRoles = requiredRoles;
        }

        public string[] RequiredRoles { get; }
    }
}
