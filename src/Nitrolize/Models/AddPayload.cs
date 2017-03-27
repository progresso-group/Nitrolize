using Nitrolize.Interfaces;

namespace Nitrolize.Models
{
    /// <summary>
    /// Stores payload data of mutations that add data.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public class AddPayload<TEntity, TInput, TId, TViewer> where TViewer : class, new() where TInput : IAddInput
    {
        public AddPayload(TEntity result, TInput input)
        {
            this.ClientMutationId = input.ClientMutationId;
            this.Viewer = new TViewer();
            this.ChangedObjectEdge = new Edge<TEntity, TId>(result);
        }

        public string ClientMutationId { get; set; }

        public Edge<TEntity, TId> ChangedObjectEdge { get; set; }

        public TViewer Viewer { get; set; }
    }
}
