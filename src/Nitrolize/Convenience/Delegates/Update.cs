using GraphQL.Types;
using Nitrolize.Interfaces;
using Nitrolize.Models;

namespace Nitrolize.Convenience.Delegates
{
    /// <summary>
    /// Delegate for mutations that update data.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="context">The ResolveFieldContext.</param>
    /// <param name="input">The input.</param>
    public delegate UpdatePayload<TInput, TEntity> Update<TInput, TEntity>(ResolveFieldContext<object> context, TInput input) where TInput : IUpdateInput;
}
