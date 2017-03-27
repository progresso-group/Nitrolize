using System;

namespace Nitrolize.Convenience.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MutationAttribute : AuthenticationRequiredAttributeBase
    {
        public MutationAttribute()
        {
        }
    }
}
