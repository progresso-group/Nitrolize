using GraphQL.Types;
using Nitrolize.Extensions;

namespace Nitrolize.Tests.Integration.Schema
{
    public class Query : ObjectGraphType
    {
        public Query()
        {
            this.Field<ViewerType>("viewer", resolve: context => new Viewer())
                .RequiresAuthentication(false);
        }
    }
}
