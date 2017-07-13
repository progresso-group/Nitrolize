using Nitrolize.Convenience.Attributes;
using Nitrolize.Convenience.Delegates;
using Nitrolize.Tests.Integration.Models;
using Nitrolize.Types.Base;

namespace Nitrolize.Tests.Integration.Schema
{
    public class Mutation : MutationBase
    {
        [Mutation]
        public Update<EntityA> UpdateEntityA => (context, input) =>
        {
            input.Name = input.Name + "_mutated";
            return input;
        };
    }
}
