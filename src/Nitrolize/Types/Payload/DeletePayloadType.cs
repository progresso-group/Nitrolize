using GraphQL.Types;

namespace Nitrolize.Types.Payload
{
    public class DeletePayloadType<T, TViewer> : ObjectGraphType where T : class where TViewer : ObjectGraphType
    {
        public DeletePayloadType()
        {
            var typeName = typeof(T).Name;
            this.Name = $"delete{typeName}Payload";

            this.Field<TViewer>("viewer");
            this.Field<BooleanGraphType>("ok");
            this.Field<NonNullGraphType<IdGraphType>>("id");

            this.Field<StringGraphType>("clientMutationId");
        }
    }
}
