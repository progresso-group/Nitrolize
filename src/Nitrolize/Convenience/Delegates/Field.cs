using GraphQL.Types;

namespace Nitrolize.Convenience.Delegates
{
    public delegate object Field<TEntity>(ResolveFieldContext<object> context);

    public delegate object Field<TEntity, TId>(ResolveFieldContext<object> context, TId id);
}
