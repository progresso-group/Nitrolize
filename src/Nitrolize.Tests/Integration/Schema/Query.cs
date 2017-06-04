using GraphQL.Types;
using Nitrolize.Extensions;
using Nitrolize.Identification;
using Nitrolize.Types.Node;
using System;

namespace Nitrolize.Tests.Integration.Schema
{
    public class Query : ObjectGraphType
    {
        public Query()
        {
            this.Field<ViewerType>("viewer", resolve: context => new Viewer())
                .RequiresAuthentication(false);

            this.Field<NodeInterfaceType>(
                "node",
                arguments: new QueryArguments(new QueryArgument<IdGraphType> { Name = "id", Description = "The node id." }),
                resolve: context =>
                {
                    // get the entity name
                    var globalId = context.GetArgument<string>("id");
                    var entityName = GlobalId.ToEntityName(globalId);

                    // switch through entity names and call matching repository or manager
                    if (entityName == "Viewer")
                    {
                        return new Viewer();
                    }

                    if (entityName == "EntityA")
                    {
                        var id = GlobalId.ToLocalId<Guid>(globalId);
                        return new EntityA { Id = id, Name = "EntityA from node" };
                    }

                    return null;
                });
        }
    }
}
