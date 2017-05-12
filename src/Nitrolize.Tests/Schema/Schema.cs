namespace Nitrolize.Tests.Schema
{
    public class Schema : GraphQL.Types.Schema
    {
        public Schema()
        {
            this.Query = new Query();
            this.Mutation = new Mutation();
        }
    }
}
