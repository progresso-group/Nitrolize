using GraphQL.Types;
using Nitrolize.Convenience;
using Nitrolize.Convenience.Attributes;
using Nitrolize.Extensions;
using Nitrolize.Identification;
using Nitrolize.Types.Node;
using System;
using System.Linq;
using System.Reflection;

namespace Nitrolize.Types.Base
{
    /// <summary>
    /// Base class for a ViewerType.
    /// Converts all attributed properties of the ViewerType to Connections and Fields automatically.
    /// </summary>
    public abstract class ViewerTypeBase : NodeType
    {
        protected ViewerTypeBase()
        {
            this.FindAndConvertPropertiesToFields();
            this.FindAndConvertPropertiesToLists();
            this.FindAndConvertPropertiesToConnections();
        }

        private void FindAndConvertPropertiesToFields()
        {
            // find all field properties
            var fields = this.GetPropertiesWithAttribute<FieldAttribute>();
            foreach (var field in fields)
            {
                var genericArguments = field.PropertyType.GetGenericArguments();

                // handle Field<TEntity>
                if (genericArguments.Count() == 1)
                {
                    this.ConvertPropertyToSimpleField(field);
                    continue;
                }

                // handle Field<TEntity, TId>
                if (genericArguments.Count() == 2)
                {
                    this.ConvertPropertyToGetByIdField(field);
                    continue;
                }
            }
        }

        private void ConvertPropertyToSimpleField(PropertyInfo field)
        {
            var genericArguments = field.PropertyType.GetGenericArguments();
            var graphType = genericArguments[0].MapToGraphType();

            // get arguments from attributes
            var arguments = new QueryArguments(field.GetQueryArguments());

            // construct resolving method
            Func<ResolveFieldContext<object>, object> resolve = (context) =>
            {
                return ((Delegate)(field.GetValue(this))).GetMethodInfo().Invoke(this, new object[] { context });
            };

            // handle authentication and authorization
            var isAuthenticationRequired = field.GetAttribute<FieldAttribute>().IsAuthenticationRequired;
            var requiredRoles = field.GetRequiredRoles();

            var graphQLField = this.Field(graphType, field.Name.ToFirstLower(), null, arguments, resolve);
            graphQLField.RequiresRoles(requiredRoles);
            graphQLField.RequiresAuthentication(isAuthenticationRequired);
        }

        private void ConvertPropertyToGetByIdField(PropertyInfo field)
        {
            var genericArguments = field.PropertyType.GetGenericArguments();
            var entityType = genericArguments[0];
            var idType = genericArguments[1];

            // construct resolving method
            Func<ResolveFieldContext<object>, object> resolve = (context) =>
            {
                // get the connection delegate
                var method = ((Delegate)(field.GetValue(this)));

                // convert the global id to local id
                var globalId = context.GetArgument<string>(entityType.GetIdPropertyName().ToFirstLower());
                var id = GlobalId.ToLocalId(idType, globalId);

                // invoke field with "context" and "id"
                return method.DynamicInvoke(new object[] { context, id });
            };

            // handle authentication and authorization
            var isAuthenticationRequired = field.GetAttribute<FieldAttribute>().IsAuthenticationRequired;
            var requiredRoles = field.GetRequiredRoles();

            var graphQLField = this.AddSingleField(entityType, resolve, field.Name.ToFirstLower());
            graphQLField.RequiresRoles(requiredRoles);
            graphQLField.RequiresAuthentication(isAuthenticationRequired);
        }

        private void FindAndConvertPropertiesToLists()
        {
            // find all list properties
            var lists = this.GetPropertiesWithAttribute<ListAttribute>();
            foreach (var list in lists)
            {
                // construct ListGraphType<EntityType>
                var entityType = list.PropertyType.GetGenericArguments()[0].MapToGraphType();
                var listGraphType = typeof(ListGraphType<>).MakeGenericType(entityType);

                // get arguments from attributes
                var arguments = new QueryArguments(list.GetQueryArguments());

                // construct resolving method
                Func<ResolveFieldContext<object>, object> resolve = (context) =>
                {
                    var method = ((Delegate)(list.GetValue(this)));
                    return method.DynamicInvoke(new object[] { context });
                };

                // handle authentication and authorization
                var isAuthenticationRequired = list.GetAttribute<ListAttribute>().IsAuthenticationRequired;
                var requiredRoles = list.GetRequiredRoles();

                var graphQLField = this.Field(listGraphType, list.Name.ToFirstLower(), null, arguments, resolve);
                graphQLField.RequiresRoles(requiredRoles);
                graphQLField.RequiresAuthentication(isAuthenticationRequired);
            }
        }

        private void FindAndConvertPropertiesToConnections()
        {
            // find all connection properties
            var connections = this.GetPropertiesWithAttribute<ConnectionAttribute>();
            foreach (var connection in connections)
            {
                var entityType = connection.PropertyType.GetGenericArguments()[0];

                // get arguments from attributes
                var arguments = connection.GetQueryArguments();

                // construct resolving method
                Func<ResolveFieldContext<object>, object> resolve = (context) =>
                {
                    // get the connection delegate
                    var method = ((Delegate)(connection.GetValue(this)));

                    // get order by infos
                    var orderByValue = context.GetArgument<int?>("orderBy");
                    var orderByPropertyName = string.Empty;
                    var orderAsc = false;

                    if (orderByValue != null)
                    {
                        var additionsType = Extensions.TypeExtensions.CreateAdditionsTypeForType(entityType, arguments.Select(a => a.Name).ToArray());
                        var orderByType = typeof(OrderByType<,>).MakeGenericType(entityType, additionsType);
                        var orderBy = (EnumerationGraphType)Activator.CreateInstance(orderByType, new object[] { });

                        var orderByIdentifier = orderBy.Values.First(v => (int)v.Value == orderByValue.Value);
                        orderByPropertyName = orderByIdentifier.Name.Split('_')[0];
                        orderAsc = orderByIdentifier.Name.Split('_')[1] == "ASC";
                    }

                    // construct parameters
                    var parametersType = typeof(Parameters);
                    var parameters = Activator.CreateInstance(parametersType, new object[]
                    {
                            context.GetArgument<string>("after"),
                            context.GetArgument<int?>("first"),
                            context.GetArgument<string>("before"),
                            context.GetArgument<int?>("last"),
                            orderByPropertyName,
                            orderAsc
                    });

                    // invoke connection with "context" and "parameters"
                    return method.DynamicInvoke(new object[] { context, parameters });
                };

                // handle authentication and authorization
                var isAuthenticationRequired = connection.GetAttribute<ConnectionAttribute>().IsAuthenticationRequired;
                var requiredRoles = connection.GetRequiredRoles();

                var graphQLField = this.AddConnectionField(entityType, resolve, connection.Name.ToFirstLower(), arguments.ToArray());
                graphQLField.RequiresRoles(requiredRoles);
                graphQLField.RequiresAuthentication(isAuthenticationRequired);
            }
        }
    }
}
