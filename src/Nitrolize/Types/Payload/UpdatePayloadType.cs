using GraphQL.Types;

namespace Nitrolize.Types.Payload
{
    public class UpdatePayloadType<T, TT> : ObjectGraphType where T : class where TT : ObjectGraphType
    {
        public UpdatePayloadType()
        {
            var typeName = typeof(T).Name;

            this.Name = $"update{typeName}Payload";

            this.Field<TT>("changedObject");
            this.Field<StringGraphType>("clientMutationId");
        }
    }
}
