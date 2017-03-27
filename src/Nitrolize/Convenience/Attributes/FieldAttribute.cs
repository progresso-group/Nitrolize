using System;

namespace Nitrolize.Convenience.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldAttribute : AuthenticationRequiredAttributeBase
    {
        public FieldAttribute()
        {
        }
    }
}
