using System;

namespace Nitrolize.Convenience.Attributes
{
    /// <summary>
    /// Speficies that the following property is a relay compatible connection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ConnectionAttribute : AuthenticationRequiredAttributeBase
    {
        public ConnectionAttribute()
        {
        }
    }
}
