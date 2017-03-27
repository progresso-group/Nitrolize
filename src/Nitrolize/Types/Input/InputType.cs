using GraphQL.Types;
using Nitrolize.Extensions;
using Nitrolize.Interfaces;

namespace Nitrolize.Types.Input
{
    public abstract class InputType<T> : InputObjectGraphType, IGraphType<T> where T : class
    {
        protected InputType() : base()
        {
            this.Name = $"{typeof(T).Name}Input".ToFirstLower();

            this.Field<StringGraphType>("clientMutationId");
            this.AddFieldsFromType<T>(omitIdProperty: false);
        }

        protected InputType(bool omitIdProperty) : base()
        {
            this.Name = $"{typeof(T).Name}Input".ToFirstLower();

            this.Field<StringGraphType>("clientMutationId");
            this.AddFieldsFromType<T>(omitIdProperty);
        }
    }
}
