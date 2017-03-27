using System;

namespace Nitrolize.Convenience.Attributes
{
    public abstract class AuthenticationRequiredAttributeBase : Attribute
    {
        public bool IsAuthenticationRequired { get; set; } = true;
    }
}
