using GraphQL.Types;

namespace Nitrolize.Types.Input
{
    public class DeleteInputType<T> : InputObjectGraphType where T : class
    {
        public DeleteInputType()
        {
            var typeName = typeof(T).Name;
            this.Name = $"delete{typeName}Input";

            this.Field<NonNullGraphType<IdGraphType>>("id");
            this.Field<StringGraphType>("clientMutationId");
        }
    }
}
