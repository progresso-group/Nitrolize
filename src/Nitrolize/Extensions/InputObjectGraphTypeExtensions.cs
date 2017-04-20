using GraphQL.Types;
using Nitrolize.Types.Input;
using System.Reflection;

namespace Nitrolize.Extensions
{
    public static class InputObjectGraphTypeExtensions
    {
        public static void AddFieldsFromType<T>(this InputObjectGraphType type, bool omitIdProperty)
        {
            var properties = typeof(T).GetExactProperies();
            foreach (var property in properties)
            {
                var typeName = property.PropertyType.Name;
                var propertyName = property.Name.ToFirstLower();

                // create IdGraphType field for Id property
                if (propertyName == "id")
                {
                    // skip id property if desired
                    if (omitIdProperty)
                    {
                        continue;
                    }

                    type.Field<IdGraphType>("id", "The global unique id of an object");
                    continue;
                }

                // create primitive fields like ints, strings, floats etc.
                if (property.PropertyType.IsSimpleType())
                {
                    type.CreateFieldFromPrimitiveProperty(property);
                    continue;
                }

                // generate list field for collection / list
                if (property.PropertyType.GetTypeInfo().GetInterface("ICollection") != null && property.PropertyType.IsGenericType())
                {
                    // create an InputType<itemType>
                    var itemType = property.PropertyType.GenericTypeArguments[0];
                    var graphQLType = typeof(InputType<>);
                    graphQLType = graphQLType.MakeGenericType(itemType);
                    graphQLType = graphQLType.ConvertToVirtualType();

                    // call AddListField like type.AddListField<Order, InputType<Order>>("orders")
                    var method = typeof(ComplexGraphTypeExtensions).GetMethod("AddListField");
                    method = method.MakeGenericMethod(new[] { itemType, graphQLType });
                    method.Invoke(type, new object[] { type, propertyName });
                }
            }
        }
    }
}
