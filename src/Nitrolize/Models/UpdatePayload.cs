using Nitrolize.Interfaces;

namespace Nitrolize.Models
{
    public class UpdatePayload<TInput, TEntity> where TInput : IUpdateInput
    {
        public UpdatePayload(TInput input, TEntity result)
        {
            this.ClientMutationId = input.ClientMutationId;
            this.ChangedObject = result;
        }

        public string ClientMutationId { get; set; }

        public TEntity ChangedObject { get; set; }
    }
}
