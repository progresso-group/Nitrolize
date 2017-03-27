using GraphQL.Types;
using Nitrolize.Interfaces;
using Nitrolize.Types;
using System;
using System.Linq;
using System.Reflection;

namespace Nitrolize.Extensions
{
    public static class ComplexGraphTypeExtensions
    {
        /// <summary>
        /// Adds a field with a name of the model type and an id field of type <see cref="IntGraphType"/>.
        /// Example usage:
        /// <code>
        ///     this.AddSingleField<Order, ObjectType<Order>>(context => {
        ///         var id = Guid.Parse(context.GetArgument<string>("id"));
        ///         var order = orderRepository.GetOrderById(id);
        ///         return order;
        ///     });
        /// </code>
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <typeparam name="TT">The type of the GraphQL type (i.e. ObjectType<Model>)</typeparam>
        /// <param name="type"></param>
        /// <param name="resolve">The resolving function that retunrs a <see cref="T"/>.</param>
        public static void AddSingleField<T, TT>(this ComplexGraphType<object> type, Func<ResolveFieldContext<object>, T> resolve) where T : class, new() where TT : ComplexGraphType<object>, IGraphType<T>
        {
            var lowerCaseName = typeof(T).Name.ToFirstLower();
            AddSingleField<T, TT, IntGraphType>(type, lowerCaseName + "Id", resolve);
        }

        /// <summary>
        /// Adds a field that gets an entity by id.
        /// Example usage:
        /// <code>
        ///     this.AddSingleField<Order, NodeObjectType<Order>, IdGraphType>("id", context => {
        ///         var id = Guid.Parse(context.GetArgument<string>("id"));
        ///         var order = orderRepository.GetOrderById(id);
        ///         return order;
        ///     });
        /// </code>
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityGraphType">The type of the entity's GraphQL type (i.e. NodeObjectType<Entity>)</typeparam>
        /// <typeparam name="TId">The type of the id field (i.e. IdGraphType or IntGraphType)</typeparam>
        /// <param name="type"></param>
        /// <param name="idName">The name of the id property.</param>
        /// <param name="resolve">The resolving function that returns a <see cref="TEntity"/>.</param>
        /// <param name="name">The name of the field.</param>
        public static FieldType AddSingleField<TEntity, TEntityGraphType, TId>(this ComplexGraphType<object> type, string idName, Func<ResolveFieldContext<object>, object> resolve, string name = null) where TEntity : class, new() where TEntityGraphType : ComplexGraphType<object>, IGraphType<TEntity> where TId : GraphType
        {
            // create name if non is given
            var lowerCaseName = name != null ? name : $"{typeof(TEntity).Name.ToFirstLower()}";

            var arguments = new QueryArguments(
                    new QueryArgument<NonNullGraphType<TId>>
                    {
                        Name = idName,
                        Description = $"id of a {lowerCaseName}"
                    });

            return type.Field(typeof(TEntityGraphType).ConvertToVirtualType(), lowerCaseName, $"The {lowerCaseName}", arguments, resolve);
        }

        /// <summary>
        /// Adds a field that gets an entity by id. Shorthand for <see cref="AddSingleField<TEntity, TEntityGraphType, TId>" />.
        /// </summary>
        public static FieldType AddSingleField(this ComplexGraphType<object> type, Type entityType, Func<ResolveFieldContext<object>, object> resolve, string name)
        {
            // construct type parameters
            var entityGraphType = typeof(NodeObjectType<>).MakeGenericType(entityType).ConvertToVirtualType();
            var idType = entityType.GetIdType().MapToGraphType();
            var idName = entityType.GetIdPropertyName().ToFirstLower();
            var method = typeof(ComplexGraphTypeExtensions).GetMethods().Where(m => m.Name == "AddSingleField" && m.IsGenericMethod && m.GetGenericArguments().Count() == 3).First();
            method = method.MakeGenericMethod(entityType, entityGraphType, idType);

            // call like AddSingleField<TEntity, TEntityGraphType, TId>(...)
            return (FieldType)method.Invoke(null, new object[] { type, idName, resolve, name });
        }

        /// <summary>
        /// Adds a list field.
        /// Example usage:
        /// <code>
        ///     this.AddListfield<Order, ObjectType<Order>>()
        /// </code>
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <typeparam name="TT">The type of the GraphQL type (i.e. ObjectType<Model>)</typeparam>
        /// <param name="type"></param>
        /// <param name="name">The name of the field. When no name is given, it is derived from the model type name.</param>
        public static FieldType AddListField<T, TT>(this ComplexGraphType<object> type, string name = null) where T : class where TT : ComplexGraphType<object>, IGraphType<T>
        {
            var lowerCaseName = name != null ? name : $"{typeof(T).Name.ToFirstLower()}s";
            return type.Field<ListGraphType<TT>>(lowerCaseName);
        }

        /// <summary>
        /// Adds a connection field.
        /// Example usage:
        /// <code>
        ///     this.AddConnectionField<Order, ObjectType<Order>>(context =>
        ///     {
        ///         return new Connection<Order>(orderRepository.GetOrders());
        ///     });
        /// </code>
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <typeparam name="TT">The type of the GraphQL type (i.e. ObjectType<Model>)</typeparam>
        /// <param name="type"></param>
        /// <param name="resolve">The resolving func that retuns a connection model.</param>
        /// <param name="name">The name of the field.</param>
        public static FieldType AddConnectionField<T, TT>(this ComplexGraphType<object> type, Func<ResolveFieldContext<object>, object> resolve, string name = null, QueryArgument[] arguments = null) where T : class where TT : ComplexGraphType<object>, IGraphType<T>
        {
            // create name if non is given
            var lowerCaseName = name != null ? name : $"{typeof(T).Name.ToFirstLower()}s";

            // build argument list... first default ones
            var args = new QueryArguments(
                       new QueryArgument<IdGraphType> { Name = "after" },
                       new QueryArgument<IntGraphType> { Name = "first" },
                       new QueryArgument<IdGraphType> { Name = "before" },
                       new QueryArgument<IntGraphType> { Name = "last" });

            // construct order by type
            // create a class type "additionsType" and add a property for each item in "arguments"
            // if any custom arguments are set
            if (arguments != null)
            {
                var additionsType = TypeExtensions.CreateAdditionsTypeForType(typeof(T), arguments.Select(a => a.Name).ToArray());

                // create a type like OrderByType<T, additionsType> and use it below
                var orderByType = typeof(OrderByType<,>).MakeGenericType(new[] { typeof(T), additionsType });
                args.Add(new QueryArgument(orderByType) { Name = "orderBy" });
            }
            else
            {
                args.Add(new QueryArgument<OrderByType<T>> { Name = "orderBy" });
            }

            // ... then add an optional argument for each property of T
            var properties = typeof(T).GetExactProperies();
            foreach (var property in properties.Where(p => p.PropertyType.IsSimpleType()))
            {
                var propertyName = property.Name.ToFirstLower();
                args.Add(new QueryArgument(property.PropertyType.MapToGraphType()) { Name = propertyName });
            }

            // .. then add custom added arguments
            if (arguments != null)
            {
                args.AddRange(arguments);
            }

            // create field
            return type.Field<ConnectionType<T, TT>>(lowerCaseName, $"The {lowerCaseName} connection.", args, resolve);
        }

        /// <summary>
        /// Adds a connection field. Shorthand for <see cref="AddConnectionField<TEntity, TEntityGraphType>" />.
        /// </summary>
        public static FieldType AddConnectionField(this ComplexGraphType<object> type, Type entityType, Func<ResolveFieldContext<object>, object> resolve, string name, params QueryArgument[] arguments)
        {
            // construct type parameters
            var entityGraphType = typeof(NodeObjectType<>).MakeGenericType(entityType).ConvertToVirtualType();
            var method = typeof(ComplexGraphTypeExtensions).GetMethods().Where(m => m.Name == "AddConnectionField" && m.IsGenericMethod).First();
            method = method.MakeGenericMethod(entityType, entityGraphType);

            // call like AddConnectionField<TEntity, TEntityGraphType>(...)
            return (FieldType)method.Invoke(null, new object[] { type, resolve, name, arguments });
        }

        /// <summary>
        /// Creats a field from a given primitive property (like string, double, int etc.).
        /// </summary>
        /// <param name="type"></param>
        /// <param name="property">The property type to create a field for.</param>
        public static void CreateFieldFromPrimitiveProperty(this ComplexGraphType<object> type, PropertyInfo property)
        {
            var propertyName = property.Name.ToFirstLower();
            type.Field(property.PropertyType.MapToGraphType(), propertyName);
        }
    }
}
