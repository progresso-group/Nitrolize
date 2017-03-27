using GraphQL.Types;

namespace Nitrolize.Convenience.Delegates
{
    /// <summary>
    /// Delegate for mutations that add data.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TViewerType">The type of the viewer type.</typeparam>
    /// <param name="context">The ResolveFieldContext.</param>
    /// <param name="input">The input.</param>
    public delegate object Add<TInput, TEntity, TViewerType>(ResolveFieldContext<object> context, TInput input);
}
