using Nitrolize.Identification;

namespace Nitrolize.Convenience
{
    /// <summary>
    /// Auto filled parameter input for connection resolve methods.
    /// Contains queried values about paging and sorting.
    /// </summary>
    public class Parameters
    {
        private string after;
        private string before;

        public Parameters(string after, int? first, string before, int? last, string orderByProperty, bool orderAsc)
        {
            this.after = after;
            this.First = first.HasValue ? first.Value : 0;
            this.before = before;
            this.Last = last.HasValue ? last.Value : 0;

            this.OrderDirection = orderAsc ? Direction.Asc : Direction.Desc;
            this.OrderByProperty = orderByProperty;
        }

        public TId GetAfter<TId>()
        {
            return this.after != null ? GlobalId.ToLocalId<TId>(this.after) : default(TId);
        }

        public int First { get; set; }

        public TId GetBefore<TId>()
        {
            return this.before != null ? GlobalId.ToLocalId<TId>(this.before) : default(TId);
        }

        public int Last { get; set; }

        public string OrderByProperty { get; set; }

        public Direction OrderDirection { get; set; }

        public enum Direction { Asc, Desc }
    }
}
