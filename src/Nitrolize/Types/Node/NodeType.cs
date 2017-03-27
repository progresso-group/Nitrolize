using GraphQL.Types;
using Nitrolize.Extensions;

namespace Nitrolize.Types.Node
{
    public class NodeType : ObjectGraphType
    {
        public NodeType()
        {
            this.Interface<NodeInterfaceType>();
            this.AddIdField();
            this.IsTypeOf = value => true;
        }
    }

    public class NodeType<T> : NodeType where T : class
    {
        public NodeType()
        {
            this.Name = typeof(T).Name;
            this.IsTypeOf = value => value is T;
        }
    }
}
