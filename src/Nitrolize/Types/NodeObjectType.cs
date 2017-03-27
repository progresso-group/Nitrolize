using Nitrolize.Extensions;
using Nitrolize.Interfaces;
using Nitrolize.Types.Node;

namespace Nitrolize.Types
{
    public class NodeObjectType<T> : NodeType<T>, IGraphType<T> where T : class
    {
        public NodeObjectType() : base()
        {
            this.AddFieldsFromType<T>();
        }
    }
}
