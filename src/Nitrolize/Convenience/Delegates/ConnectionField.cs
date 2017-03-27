using GraphQL.Types;

namespace Nitrolize.Convenience.Delegates
{
    /// <summary>
    /// Delegate for connection fields.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="context">The ResolveFieldContext.</param>
    /// <param name="parameters">Parameters like After, First, Before, Last.</param>
    public delegate object ConnectionField<TEntity>(ResolveFieldContext<object> context, Parameters parameters);
}
