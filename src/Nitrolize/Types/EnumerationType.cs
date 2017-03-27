using GraphQL.Types;
using System;
using System.Linq;

namespace Nitrolize.Types
{
    /// <summary>
    /// Helper class to create an Relay compatible EnumerationGraphType class of an enum.
    /// Automatically creates all fields for each enum value.
    /// </summary>
    /// <typeparam name="T">The enum to create the EnumerationType for.</typeparam>
    public class EnumerationType<T> : EnumerationGraphType where T : struct
    {
        public EnumerationType()
        {
            this.Name = typeof(T).Name;

            var names = Enum.GetNames(typeof(T)).ToList();
            var values = Enum.GetValues(typeof(T)).Cast<int>().ToList();

            foreach (var name in names)
            {
                var index = names.IndexOf(name);

                this.AddValue(name, string.Empty, values[index]);
            }
        }
    }
}
