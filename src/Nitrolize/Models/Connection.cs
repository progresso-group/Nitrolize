using GraphQL.Types.Relay.DataObjects;
using System.Collections.Generic;
using System.Linq;

namespace Nitrolize.Models
{
    public class Connection<T, TId>
    {
        public Connection(IList<T> items)
        {
            this.Edges = items != null ? items.Select(i => new Edge<T, TId>(i)).ToList() : new List<Edge<T, TId>>();
            this.Count = this.Edges.Count;

            this.PageInfo = new PageInfo
            {
                StartCursor = this.Edges.Any() ? this.Edges.First().Cursor.ToString() : null,
                EndCursor = this.Edges.Any() ? this.Edges.Last().Cursor.ToString() : null,
            };
        }

        public IList<Edge<T, TId>> Edges { get; set; }

        public float Count { get; set; }

        public PageInfo PageInfo { get; set; }
    }
}
