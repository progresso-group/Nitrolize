using Nitrolize.Extensions;
using System;

namespace Nitrolize.Convenience.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ArgumentAttribute : Attribute
    {
        public ArgumentAttribute(string name, Type argumentType)
        {
            this.ArgumentType = argumentType.MapToGraphType();
            this.Name = name;
        }

        public Type ArgumentType { get; }

        public string Name { get; }
    }
}
