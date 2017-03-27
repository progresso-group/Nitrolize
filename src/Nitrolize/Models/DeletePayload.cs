using Nitrolize.Identification;

namespace Nitrolize.Models
{
    public class DeletePayload<TId, TViewer> where TViewer : class, new()
    {
        public DeletePayload(DeleteInput input, bool ok)
        {
            this.Id = GlobalId.ToLocalId<TId>(input.Id);
            this.ClientMutationId = input.ClientMutationId;
            this.Viewer = new TViewer();
            this.Ok = ok;
        }

        public TId Id { get; private set; }
        public bool Ok { get; private set; }

        public string ClientMutationId { get; private set; }
        public TViewer Viewer { get; private set; }
    }
}
