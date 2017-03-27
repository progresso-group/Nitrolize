using GraphQL.Types;
using GraphQL.Types.Relay;
using Nitrolize.Interfaces;

namespace Nitrolize.Types
{
    /// <summary>
    /// Helper class for creating Relay compatible connections.
    /// Adds automatically a "count" field.
    /// Provides parameterized constructor for easy Name setting.
    /// Adds automatically "nodes" field.
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    /// <typeparam name="TT">The type of the models graph type.</typeparam>
    public class ConnectionType<T, TT> : ConnectionType<TT> where T : class where TT : ComplexGraphType<object>, IGraphType<T>
    {
        public ConnectionType()
        {
            this.Name = typeof(T).Name + "Connection";
            this.Description = $"A connection to a list of items ({typeof(TT).Name}, {typeof(T).Name}).";

            this.Field<FloatGraphType>("count");
        }
    }
}
