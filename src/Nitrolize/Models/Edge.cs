using Nitrolize.Extensions;
using Nitrolize.Identification;

namespace Nitrolize.Models
{
    public class Edge<T, TId>
    {
        public Edge(T node)
        {
            this.Node = node;
            this.Cursor = GlobalId.ToGlobalId(node.GetType().Name, typeof(T).GetId<TId>(node).ToString());
        }

        public string Cursor { get; set; }

        public T Node { get; set; }
    }
}
