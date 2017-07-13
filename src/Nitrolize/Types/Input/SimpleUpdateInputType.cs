using GraphQL.Types;
using Nitrolize.Extensions;
using Nitrolize.Interfaces;

namespace Nitrolize.Types.Input
{
    public class SimpleUpdateInputType<T> : InputObjectGraphType, IGraphType<T> where T : class
    {
        public SimpleUpdateInputType() : base()
        {
            this.Name = $"update{typeof(T).Name}Input".ToFirstLower();
            this.AddFieldsFromType<T>(omitIdProperty: false);
        }
    }
}
