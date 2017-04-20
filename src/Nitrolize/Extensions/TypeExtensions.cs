using GraphQL.Types;

#if NETCOREAPP1_0

using Nitrolize.Polyfills;

#endif

using Nitrolize.Types;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Nitrolize.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Returns only those properties, that are defined exact for the given type. Does not
        /// return properties of inherited types.
        /// </summary>
        /// <param name="type">The type of which the properties are desired.</param>
        /// <returns></returns>
        public static PropertyInfo[] GetExactProperies(this Type type)
        {
            return type.GetProperties().Where(p => p.DeclaringType == type).ToArray();
        }

        /// <summary>
        /// Returns the virtual type of the given type.
        /// A virtual type is a dynamically generated type that extends the given type.
        /// </summary>
        /// <param name="type">The type from which the virtual type is created from.</param>
        /// <returns>The virtual type of the given type.</returns>
        public static Type ConvertToVirtualType(this Type type)
        {
            // get the type's virtual type name
            var typeName = type.GetVirtualTypeName();

            // check if type / assembly already exists
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.Contains($".{typeName},"));
            if (assembly != null)
            {
                return assembly.GetType(typeName);
            }

            var moduleBuilder = CreateVirtualModuleBuilderForType(type);

            // define and return the new type
            TypeBuilder typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public, type);
            return typeBuilder.CreateTypeInfo().AsType();
        }

        private static ModuleBuilder CreateVirtualModuleBuilderForType(Type type)
        {
            var typeName = type.GetVirtualTypeName();

            // create an assembly for the type
            var assemblyName = new AssemblyName();
            assemblyName.Name = $"GraphQL.VirtualTypes.{typeName}";
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            return assemblyBuilder.DefineDynamicModule("GraphQLVirtualTypesModule");
        }

        private static ModuleBuilder CreateVirtualModuleBuilderForAdditionType()
        {
            // create an assembly for the type
            var assemblyName = new AssemblyName();
            assemblyName.Name = "GraphQL.VirtualTypes.AdditionTypes";
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            return assemblyBuilder.DefineDynamicModule("GraphQLVirtualTypesModule");
        }

        public static string GetVirtualTypeName(this Type type)
        {
            // if the type is of form GenericClass<SomeClass>
            // then typeName is constructed as 'GenericClassSomeClass'.
            if (type.IsGenericType())
            {
                var genericTypeName = type.GetGenericTypeDefinition().Name;
                genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
                var argumentTypeName = type.GetGenericArguments()[0].Name;
                return genericTypeName + argumentTypeName;
            }

            // otherwise it's regular name is used
            return type.Name;
        }

        public static Type CreateAdditionsTypeForType(Type modelType, params string[] arguments)
        {
            var moduleBuilder = CreateVirtualModuleBuilderForAdditionType();
            var typeBuilder = moduleBuilder.DefineType($"{modelType.Name}Addition", TypeAttributes.Public);

            foreach (var argument in arguments)
            {
                typeBuilder.AddProperty(argument, typeof(string), false);
            }

            return typeBuilder.CreateTypeInfo().AsType();
        }

        /// <summary>
        /// Converts a Type to an input type by converting Guid and int properties to
        /// string properties. This is used for incoming client data where ids are
        /// encoded as GlobalIds.
        /// For AddInputTypes, this method can be used to remove the id property to get
        /// a Relay compatible Input Type for Add Mutations.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="removeIdProperty">If set to true, the id property of the type is removed.</param>
        /// <returns>A type with string ids or without an id property.</returns>
        public static Type ConvertToInputType(this Type type, bool removeIdProperty = false)
        {
            var typeName = type.GetVirtualTypeName();
            var moduleBuilder = CreateVirtualModuleBuilderForType(type);
            var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public);

            Type idPropertyType = null;
            foreach (var property in type.GetProperties())
            {
                var propertyHasOnyGetter = property.GetGetMethod() != null;

                // convert Id properties to a property of type string
                if (property.Name.EndsWith("Id"))
                {
                    if (property.Name == "Id" || property.Name == $"{type.Name}Id")
                    {
                        if (!removeIdProperty)
                        {
                            idPropertyType = property.PropertyType;
                            typeBuilder.AddProperty(property.Name, typeof(string), propertyHasOnyGetter);
                        }
                    }
                    else
                    {
                        typeBuilder.AddProperty(property.Name, typeof(string), propertyHasOnyGetter);
                    }

                    continue;
                }

                // handle collections with items
                if (property.PropertyType.GetTypeInfo().GetInterface("ICollection") != null && property.PropertyType.IsGenericType())
                {
                    var itemType = property.PropertyType.GenericTypeArguments[0];
                    itemType = itemType.ConvertToInputType();
                    var collectionType = property.PropertyType.GetGenericTypeDefinition().MakeGenericType(new[] { itemType });

                    typeBuilder.AddProperty(property.Name, collectionType, propertyHasOnyGetter);

                    continue;
                }

                typeBuilder.AddProperty(property.Name, property.PropertyType, propertyHasOnyGetter);
            }

            // store original id type in custom attribute of the class. For a Guid id it would look like this:
            // [TypeDescriptionProvider(typeof(Guid))]
            // public class ...
            if (!removeIdProperty)
            {
                var constructorInfo = typeof(TypeDescriptionProviderAttribute).GetConstructor(new[] { typeof(Type) });
                var customAttributeBuilder = new CustomAttributeBuilder(constructorInfo, new[] { idPropertyType });
                typeBuilder.SetCustomAttribute(customAttributeBuilder);
            }

            return typeBuilder.CreateTypeInfo().AsType();
        }

        private static void AddProperty(this TypeBuilder typeBuilder, string name, Type type, bool hasOnlyGetter)
        {
            // define property
            var propertyBuilder = typeBuilder.DefineProperty(name, PropertyAttributes.None, type, null);

            // define field
            FieldBuilder fieldBuilder = typeBuilder.DefineField($"<{name}>k__BackingField", type, FieldAttributes.Private);

            // define "getter" method
            var getterBuilder = typeBuilder.DefineMethod($"get_{name}",
                                                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                                                type,
                                                Type.EmptyTypes);
            ILGenerator getterIL = getterBuilder.GetILGenerator();
            getterIL.Emit(OpCodes.Ldarg_0);
            getterIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getterIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getterBuilder);

            MethodBuilder setterBuilder = typeBuilder.DefineMethod($"set_{name}",
                                                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                                                null,
                                                new Type[] { type });
            ILGenerator setterIL = setterBuilder.GetILGenerator();
            setterIL.Emit(OpCodes.Ldarg_0);
            setterIL.Emit(OpCodes.Ldarg_1);
            setterIL.Emit(OpCodes.Stfld, fieldBuilder);
            setterIL.Emit(OpCodes.Ret);

            propertyBuilder.SetSetMethod(setterBuilder);
        }

        /// <summary>
        /// Checks if a type has an Id property.
        /// Checks for properties named "Id" and "[TypeName]Id". 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool HasId(this Type type)
        {
            try
            {
                type.GetIdPropertyName();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the type of the Id property of a given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Is thrown, if no Id property was found.</exception>
        public static Type GetIdType(this Type type)
        {
            var idPropertyInfo = type.GetIdPropertyInfo();
            return idPropertyInfo.PropertyType;
        }

        /// <summary>
        /// Returns the value of the Id property of a given type and instance, if any is found.
        /// </summary>
        /// <typeparam name="TId"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Is thrown, if no Id property was found or could not be casted into the desired TId type.</exception>
        public static TId GetId<TId>(this Type type, object instance)
        {
            // check for property named "Id"
            var idPropertyInfo = type.GetIdPropertyInfo();

            // try to get the value of the property as a TId instance
            TId id;
            try
            {
                id = (TId)idPropertyInfo.GetValue(instance);
            }
            catch
            {
                throw new Exception($"The Id property of {type.Name} is of type {idPropertyInfo.PropertyType.Name} and cannot be casted to {typeof(TId).Name}.");
            }

            return id;
        }

        /// <summary>
        /// Gets the PropertyInfo of the id property of a current type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>The property info of the id property.</returns>
        public static PropertyInfo GetIdPropertyInfo(this Type type)
        {
            return type.GetProperty(type.GetIdPropertyName());
        }

        /// <summary>
        /// Gets the name of the id property of the current type if any is found.
        /// Looks up for properties named "Id" or "[Typename]Id".
        /// </summary>
        /// <param name="type"></param>
        /// <returns>The name of the id property of the current type, if any is found.</returns>
        /// <exception cref="Exception">Is thrown when no id property was found.</exception>
        public static string GetIdPropertyName(this Type type)
        {
            // check for "Id"
            var propertyInfo = type.GetProperty("Id");
            if (propertyInfo != null)
            {
                return "Id";
            }

            // check for property named "[Typename]Id"
            propertyInfo = type.GetProperty($"{type.Name}Id");
            if (propertyInfo != null)
            {
                return $"{type.Name}Id";
            }

            throw new Exception($"Could not find any id property on {type.Name}.");
        }

        /// <summary>
        /// Maps the current type to a corresponding graph type.
        /// I.e. String => StringGraphType.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type MapToGraphType(this Type type)
        {
            var typeName = type.Name;

            if (typeName == "Boolean")
            {
                return typeof(BooleanGraphType);
            }
            if (typeName == "Guid")
            {
                return typeof(IdGraphType);
            }
            if (typeName == "String")
            {
                return typeof(StringGraphType);
            }
            if (typeName == "Int32")
            {
                return typeof(IntGraphType);
            }
            if (typeName == "Decimal")
            {
                return typeof(DecimalGraphType);
            }
            if (typeName == "Single")
            {
                return typeof(FloatGraphType);
            }
            // https://github.com/graphql-dotnet/graphql-dotnet/pull/78
            // According to the GraphQL specification a Float type is a double precision floating point.
            if (typeName == "Double")
            {
                return typeof(FloatGraphType);
            }
            if (typeName == "DateTime")
            {
                return typeof(DateGraphType);
            }

            // handle enums
            if (type.IsEnum())
            {
                // construct and return an EnumerationType<type>
                var enumerationType = typeof(EnumerationType<>);
                enumerationType = enumerationType.MakeGenericType(type);
                return enumerationType;
            }

            // handle Nullable<type> by just dropping the Nullable part
            if (Nullable.GetUnderlyingType(type) != null)
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                return underlyingType.MapToGraphType();
            }

            // handle list
            if (type.GetInterfaces().Contains(typeof(IList)) && type.IsGenericType())
            {
                var listItemType = type.GetGenericArguments()[0];
                listItemType = listItemType.MapToGraphType();
                return typeof(ListGraphType<>).MakeGenericType(listItemType);
            }

            // handle node object types
            if (type.HasId())
            {
                return typeof(NodeObjectType<>).MakeGenericType(type);
            }

            // handle object types
            return typeof(ObjectType<>).MakeGenericType(type);
            // throw new NotImplementedException($"There is no graph type mapping implemented for {type.Name}.");
        }

        public static bool IsSimpleType(this Type type)
        {
            return type.IsPrimitive() ||
                   type.IsEnum() ||
                   type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(Guid) ||
                   type == typeof(DateTime) ||
                   (Nullable.GetUnderlyingType(type) != null && Nullable.GetUnderlyingType(type).IsSimpleType());
        }

        #region Polyfills

        public static bool IsPrimitive(this Type type)
        {
#if NETCOREAPP1_0
            return type.GetTypeInfo().IsPrimitive;
#else
            return type.IsPrimitive;
#endif
        }

        public static bool IsEnum(this Type type)
        {
#if NETCOREAPP1_0
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        public static bool IsGenericType(this Type type)
        {
#if NETCOREAPP1_0
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

//#if NETCOREAPP1_0
//        public static MethodInfo[] GetMethods(this Type type)
//        {
//            return type.GetTypeInfo().GetMethods();
//        }

//        public static MethodInfo GetMethod(this Type type, string name)
//        {
//            return type.GetTypeInfo().GetMethod(name);
//        }

//        public static Type GetInterface(this Type type, string name)
//        {
//            return type.GetTypeInfo().GetInterface(name);
//        }

//#endif


#endregion
    }
}
