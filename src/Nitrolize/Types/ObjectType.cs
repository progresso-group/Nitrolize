using GraphQL.Types;
using Nitrolize.Extensions;

namespace Nitrolize.Types
{
    public class ObjectType<T> : ObjectGraphType where T : class
    {
        public ObjectType() : base()
        {
            this.Name = typeof(T).Name;
            this.IsTypeOf = value => value is T;

            this.AddFieldsFromType<T>(handleIdProperty: true);
        }
    }
}
