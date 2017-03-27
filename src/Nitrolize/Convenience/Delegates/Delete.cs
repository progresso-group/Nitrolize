using GraphQL.Types;
using Nitrolize.Models;

namespace Nitrolize.Convenience.Delegates
{
    /// <summary>
    /// Delegate for mutations that delete data.
    /// </summary>
    /// <typeparam name="TId">The type of the id.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TViewerType">The type of the viewer type.</typeparam>
    /// <param name="context">The ResolveFieldContext.</param>
    /// <param name="input">The input.</param>
    public delegate object Delete<TId, TEntity, TViewerType>(ResolveFieldContext<object> context, TId id, DeleteInput input);
}
