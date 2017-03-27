using GraphQL.Types;

namespace Nitrolize.Types.Node
{
    public class NodeInterfaceType : InterfaceGraphType
    {
        public NodeInterfaceType()
        {
            this.Name = "Node";
            this.Field<NonNullGraphType<IdGraphType>>("id", "The id of the node");
        }
    }
}
