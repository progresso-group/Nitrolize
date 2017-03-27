using GraphQL.Types;
using GraphQL.Types.Relay;

namespace Nitrolize.Types.Payload
{
    public class AddPayloadType<TEntity, TGraphEntity, TViewer> : ObjectGraphType where TEntity : class where TGraphEntity : ObjectGraphType where TViewer : ObjectGraphType
    {
        public AddPayloadType()
        {
            var typeName = typeof(TEntity).Name;
            this.Name = $"add{typeName}Payload";

            this.Field<TViewer>("viewer");
            this.Field<EdgeType<TGraphEntity>>("changedObjectEdge");

            this.Field<StringGraphType>("clientMutationId");
        }
    }
}
