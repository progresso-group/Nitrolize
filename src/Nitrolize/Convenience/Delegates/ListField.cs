using GraphQL.Types;

namespace Nitrolize.Convenience.Delegates
{
    /// <summary>
    /// Delegate for list fields.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="context">The ResolveFieldContext.</param>
    public delegate object ListField<TEntity>(ResolveFieldContext<object> context);
}
