using GraphQL;
using GraphQL.Types;
using Nitrolize.Identification;
using Nitrolize.Interfaces;
using Nitrolize.Models;
using Nitrolize.Types;
using Nitrolize.Types.Input;
using Nitrolize.Types.Payload;
using System;
using System.Linq;
using System.Reflection;

namespace Nitrolize.Extensions
{
    public static class ObjectGraphTypeExtensions
    {
        /// <summary>
        /// Adds all fields to a graph type by iterating through the model type properties.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="type"></param>
        /// <param name="handleIdProperty">If set to true, the Id property is added as field. Default is false.</param>
        public static void AddFieldsFromType<T>(this ObjectGraphType<object> type, bool handleIdProperty = false)
        {
            var properties = typeof(T).GetExactProperies();
            foreach (var property in properties)
            {
                var typeName = property.PropertyType.Name;
                var propertyName = property.Name.ToFirstLower();

                // handle Id
                if (propertyName == "id")
                {
                    // skip property named "id" if desired. May be skipped because it is handled already in NodeType.
                    if (!handleIdProperty)
                    {
                        continue;
                    }

                    type.AddIdField();
                    continue;
                }

                // convert all foreign key id fields to be global unique
                if (propertyName.EndsWith("Id"))
                {
                    type.Field<IdGraphType>(propertyName, null, null, context =>
                    {
                        var node = context.Source;
                        return GlobalId.ToGlobalId(property.Name.Remove(propertyName.Length - 2), property.GetValue(node).ToString());
                    });
                    continue;
                }

                // create primitive fields like ints, strings, floats etc.
                if (property.PropertyType.IsSimpleType())
                {
                    type.CreateFieldFromPrimitiveProperty(property);
                    continue;
                }

                // generate connection field for collection / list
                if (property.PropertyType.GetInterface("ICollection") != null && property.PropertyType.IsGenericType())
                {
                    // create an ObjectType<itemType>
                    var itemType = property.PropertyType.GenericTypeArguments[0];
                    var graphQLType = typeof(NodeObjectType<>).MakeGenericType(itemType);
                    graphQLType = graphQLType.ConvertToVirtualType();

                    // construct resolver
                    Func<ResolveFieldContext<object>, object> resolver = (context) => {
                        // get the collection / list items
                        // TODO: crazy thoughts: what if this property was not loaded yet?
                        // If, for example, a list of users is loaded by a repository, the roles
                        // of each user may not have been loaded.
                        // Could it be possibe to somehow get and call a repository here and perform
                        // something like lazy loading?
                        // Further: if a collection of object is loaded, could it be easily possible to
                        // perform batch loading and have some temporaly cache for this requests?
                        var items = context.Source.GetProperyValue(propertyName);

                        // constuct a type like Connection<Order, TId>
                        var connectionType = typeof(Connection<,>);
                        var idType = itemType.GetIdType();
                        connectionType = connectionType.MakeGenericType(new[] { itemType, idType });

                        // call like new Connection<Order, TId>(items)
                        var connection = Activator.CreateInstance(connectionType, items);
                        return connection;
                    };

                    type.AddConnectionField(itemType, resolver, propertyName, null);

                    continue;
                }

                // create complex property fields
                var complexType = typeof(NodeObjectType<>).MakeGenericType(property.PropertyType).ConvertToVirtualType();
                type.Field(complexType, propertyName);
            }
        }

        public static FieldType AddIdField(this ObjectGraphType<object> type)
        {
            return type.Field<NonNullGraphType<IdGraphType>>("id", "The global unique id of an object", null, context => {
                var node = context.Source;

                // try to find the id property (i.e. "Id" or "OrderId")
                var idPropertyInfo = node.GetType().GetIdPropertyInfo();

                // generate and return global id
                return GlobalId.ToGlobalId(node.GetType().Name, idPropertyInfo.GetValue(node).ToString());
            });
        }

        public static FieldType CreateAddMutation<TInput, TEntity, TViewer>(this ObjectGraphType<object> type, Func<ResolveFieldContext<object>, object, object> resolve, string mutationName = null) where TEntity : class where TInput : class, IAddInput, new()
        {
            // construct AddPayloadType<TEntity, ObjectType<TEntity>>
            var entityType = typeof(TEntity);
            var graphEntityType = typeof(NodeObjectType<>).MakeGenericType(entityType);
            var addPayloadType = typeof(AddPayloadType<,,>).MakeGenericType(new[] { entityType, graphEntityType, typeof(TViewer) });
            addPayloadType = addPayloadType.ConvertToVirtualType();

            // construct mutation name (i.e. "addOrder")
            var fieldName = mutationName != null ? mutationName : $"add{entityType.Name}";

            // construct NonNullableGraphType<AddInputType<T>>
            var addInputType = typeof(AddInputType<>).MakeGenericType(entityType);
            var nonNullableGraphType = typeof(NonNullGraphType<>);
            nonNullableGraphType = nonNullableGraphType.MakeGenericType(addInputType);

            // construct arguments
            var argument = new QueryArgument(nonNullableGraphType) { Name = "input" };
            var arguments = new QueryArguments(argument);

            // construct resolver
            Func<ResolveFieldContext<object>, object> resolver = (context) =>
            {
                // remove id properties from the type: do not expect new items to have ids defined
                var inputType = typeof(TInput).ConvertToInputType(removeIdProperty: true);

                // call like context.GetArgument<TInput>("input");
                var getArgumentMethod = context.GetType().GetMethod("GetArgument").MakeGenericMethod(inputType);
                var input = getArgumentMethod.Invoke(context, new[] { "input", null });

                return resolve(context, input.CloneAs<TInput>());
            };

            // call Field like type.Field<AddPayloadType<Order>>("addOrder", args, resolve)
            var method = type.GetType().GetMethods().First(m => m.Name == "Field" && m.IsGenericMethod && m.ReturnType == typeof(FieldType));
            method = method.MakeGenericMethod(addPayloadType);
            return (FieldType)method.Invoke(type, new object[] { fieldName, null, arguments, resolver, null });
        }

        public static FieldType CreateAddMutation(this ObjectGraphType<object> type, Type inputType, Type entityType, Type viewerType, Func<ResolveFieldContext<object>, object, object> resolve, string mutationName)
        {
            // construct type parameters
            var method = typeof(ObjectGraphTypeExtensions).GetMethods().Where(m => m.Name == "CreateAddMutation" && m.IsGenericMethod).First();
            method = method.MakeGenericMethod(inputType, entityType, viewerType);

            // call CreateAddMutation<TInput, TEntity>(...)
            return (FieldType)method.Invoke(null, new object[] { type, resolve, mutationName });
        }

        public static FieldType CreateUpdateMutation<TInput, TEntity>(this ObjectGraphType<object> type, Func<ResolveFieldContext<object>, object, object> resolve, string mutationName = null) where TEntity : class where TInput : class, IUpdateInput, new()
        {
            // construct UpdatePayloadType<TEntity, ObjectType<TEntity>>
            var entityType = typeof(TEntity);
            var objectType = typeof(NodeObjectType<>).MakeGenericType(entityType);
            var updatePayloadType = typeof(UpdatePayloadType<,>).MakeGenericType(new[] { entityType, objectType });
            updatePayloadType = updatePayloadType.ConvertToVirtualType();

            // construct mutation name (i.e. "updateOrder")
            var fieldName = mutationName != null ? mutationName : $"update{entityType.Name}";

            // construct NonNullableGraphType<UpdateInputType<T>>
            var updateInputType = typeof(UpdateInputType<>).MakeGenericType(entityType);
            var nonNullableGraphType = typeof(NonNullGraphType<>);
            nonNullableGraphType = nonNullableGraphType.MakeGenericType(updateInputType);

            // construct arguments
            var argument = new QueryArgument(nonNullableGraphType) { Name = "input" };
            var arguments = new QueryArguments(argument);

            // construct resolver
            Func<ResolveFieldContext<object>, object> resolver = (context) =>
            {
                // TODO: correct comment, change method names like "CloneAs":
                // These methods are to map / convert input object string ids to id / guid

                // drop duplicate Id properties: an UpdateModelInput that extends its Model
                // may have defined a new Id property of type string that should hide the Model's
                // original Id. If the original Id definition is not dropped, Reflection cannot
                // fetch the Id property because of an AmbiguousMatchException exception.
                var inputType = typeof(TInput).ConvertToInputType();
                var getArgumentMethod = context.GetType().GetMethod("GetArgument").MakeGenericMethod(inputType);
                // call like context.GetArgument<TInput>("input");
                var input = getArgumentMethod.Invoke(context, new[] { "input", null });
                return resolve(context, input.CloneAs<TInput>());
            };

            // call Field like type.Field<UpdatePayloadType<Order>>("updateOrder", args, resolve)
            var method = type.GetType().GetMethods().First(m => m.Name == "Field" && m.IsGenericMethod && m.ReturnType == typeof(FieldType));
            method = method.MakeGenericMethod(updatePayloadType);
            return (FieldType)method.Invoke(type, new object[] { fieldName, null, arguments, resolver, null });
        }

        public static FieldType CreateUpdateMutation(this ObjectGraphType<object> type, Type inputType, Type entityType, Func<ResolveFieldContext<object>, object, object> resolve, string mutationName)
        {
            // construct type parameters
            var method = typeof(ObjectGraphTypeExtensions).GetMethods().Where(m => m.Name == "CreateUpdateMutation" && m.IsGenericMethod).First();
            method = method.MakeGenericMethod(inputType, entityType);

            // call CreateUpdateMutation<TInput, TEntity>(...)
            return (FieldType)method.Invoke(null, new object[] { type, resolve, mutationName });
        }

        public static FieldType CreateDeleteMutation<TId, TEntity, TViewer>(this ObjectGraphType<object> type, Func<ResolveFieldContext<object>, object, object, object> resolve, string mutationName = null) where TEntity : class
        {
            // construct DeletePayloadType<Order>
            var entityType = typeof(TEntity);
            var deletePayloadType = typeof(DeletePayloadType<,>).MakeGenericType(entityType, typeof(TViewer));
            deletePayloadType = deletePayloadType.ConvertToVirtualType();

            // construct mutation name (i.e. "deleteOrder")
            var fieldName = mutationName != null ? mutationName : $"delete{entityType.Name}";

            // construct NonNullGraphType<DeleteInputType<Order>>
            var deleteInputType = typeof(DeleteInputType<>).MakeGenericType(entityType);
            var nonNullableGraphType = typeof(NonNullGraphType<>);
            nonNullableGraphType = nonNullableGraphType.MakeGenericType(deleteInputType);

            // construct arguments
            var argument = new QueryArgument(nonNullableGraphType) { Name = "input" };
            var arguments = new QueryArguments(argument);

            // construct resolver
            Func<ResolveFieldContext<object>, object> resolver = (context) =>
            {
                var input = context.GetArgument<DeleteInput>("input");
                var id = GlobalId.ToLocalId<TId>(input.Id);

                return resolve(context, id, input);
            };

            // call Field like type.Field<DeletePayloadType<Order>>("deleteOrder", args, resolve)
            var method = type.GetType().GetMethods().First(m => m.Name == "Field" && m.IsGenericMethod && m.ReturnType == typeof(FieldType));
            method = method.MakeGenericMethod(deletePayloadType);
            return (FieldType)method.Invoke(type, new object[] { fieldName, null, arguments, resolver, null });
        }

        public static FieldType CreateDeleteMutation(this ObjectGraphType<object> type, Type idType, Type entityType, Type viewerType, Func<ResolveFieldContext<object>, object, object, object> resolve, string mutationName)
        {
            // construct type parameters
            var method = typeof(ObjectGraphTypeExtensions).GetMethods().Where(m => m.Name == "CreateDeleteMutation" && m.IsGenericMethod).First();
            method = method.MakeGenericMethod(idType, entityType, viewerType);

            // call CreateDeleteMutation<TId, TEntity, TViewer>(...)
            return (FieldType)method.Invoke(null, new object[] { type, resolve, mutationName });
        }
    }
}
