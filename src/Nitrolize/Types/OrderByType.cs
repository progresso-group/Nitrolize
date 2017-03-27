using GraphQL.Types;
using Nitrolize.Extensions;
using System;
using System.Linq;

namespace Nitrolize.Types
{
    /// <summary>
    /// Helper class to create an Relay compatible OrderBy Enumeration Type of
    /// a given model type.
    /// </summary>
    public class OrderByType<T> : EnumerationGraphType where T : class
    {
        public OrderByType()
        {
            this.Name = $"orderBy{typeof(T).Name}";

            this.Index = 0;
            this.AddValuesForType(typeof(T));
        }

        protected int Index { get; set; }

        protected void AddValuesForType(Type type)
        {
            var properties = type.GetExactProperies();
            var propertyNames = properties.Select(p => p.Name.ToUpperInvariant()).ToArray();

            foreach (var propertyName in propertyNames)
            {
                var name = propertyName;

                if (!name.StartsWith("ID") && name.EndsWith("ID"))
                {
                    name = name.Replace("ID", string.Empty);
                }

                this.AddValue($"{name}_ASC", string.Empty, this.Index);
                this.Index++;

                this.AddValue($"{name}_DESC", string.Empty, this.Index);
                this.Index++;
            }
        }
    }

    public class OrderByType<TModel, TAdditions> : OrderByType<TModel> where TModel : class
    {
        public OrderByType() : base()
        {
            this.AddValuesForType(typeof(TAdditions));
        }
    }
}
