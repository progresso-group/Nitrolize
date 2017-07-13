using GraphQL.Types;
using Nitrolize.Convenience.Attributes;
using Nitrolize.Convenience.Delegates;
using Nitrolize.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nitrolize.Types.Base
{
    /// <summary>
    /// Base class to be used for application mutation container classes.
    /// When initialized, converts all annotated properties into Relay compatible 
    /// mutation fields using the propertie's Delegates as resolving methods.
    /// </summary>
    public abstract class MutationBase : ObjectGraphType
    {
        /// <summary>
        /// Initializes a new instance of the mutation container and the
        /// conversion of the mutation properties.
        /// </summary>
        protected MutationBase()
        {
            this.FindAndConvertPropertiesToAddMutations();
            this.FindAndConvertPropertiesToUpdateMutations();
            this.FindAndConvertPropertiesToSimpleUpdateMutations();
            this.FindAndConvertPropertiesToDeleteMutations();
        }

        /// <summary>
        /// Creates mutation fields for every property found that is annotated
        /// by the [Mutation] attribute and has the type of <see cref="Add<,>" /> Delegate.
        /// </summary>
        private void FindAndConvertPropertiesToAddMutations()
        {
            var properties = this.GetMutationPropertiesOfType(typeof(Add<,,>));
            foreach (var property in properties)
            {
                // get the input and entity type
                var genericArguments = property.PropertyType.GetGenericArguments();
                var inputType = genericArguments[0];
                var entityType = genericArguments[1];
                var viewerType = genericArguments[2];

                // create a resolve Func from the properties Delegate method:
                // the resolver expects the context and an AddInput as inputs
                Func<ResolveFieldContext<object>, object, object> resolve = (context, input) =>
                {
                    return ((Delegate)(property.GetValue(this))).GetMethodInfo().Invoke(this, new object[] { context, input });
                };

                // create the field
                var field = this.CreateAddMutation(inputType, entityType, viewerType, resolve, property.Name.ToFirstLower());
                this.SetAuthenticationAndAuthorizationConfigForField(field, property);
            }
        }

        /// <summary>
        /// Creates mutation fields for every property found that is annotated
        /// by the [Mutation] attribute and has the type of <see cref="Update<,>" /> Delegate.
        /// </summary>
        private void FindAndConvertPropertiesToUpdateMutations()
        {
            var properties = this.GetMutationPropertiesOfType(typeof(Update<,>));
            foreach (var property in properties)
            {
                // get the input and entity type
                var genericArguments = property.PropertyType.GetGenericArguments();
                var inputType = genericArguments[0];
                var entityType = genericArguments[1];

                // create a resolve Func from the properties Delegate method:
                // the resolver expects the context and an UpdateInput as inputs
                Func<ResolveFieldContext<object>, object, object> resolve = (context, input) =>
                {
                    return ((Delegate)(property.GetValue(this))).GetMethodInfo().Invoke(this, new object[] { context, input });
                };

                // create the field
                var field = this.CreateUpdateMutation(inputType, entityType, resolve, property.Name.ToFirstLower());
                this.SetAuthenticationAndAuthorizationConfigForField(field, property);
            }
        }

        /// <summary>
        /// Creates mutation fields for every property found that is annotated
        /// by the [Mutation] attribute and has the type of <see cref="Update<>" /> Delegate.
        /// </summary>
        private void FindAndConvertPropertiesToSimpleUpdateMutations()
        {
            var properties = this.GetMutationPropertiesOfType(typeof(Update<>));
            foreach (var property in properties)
            {
                // get the type arguments
                var genericArguments = property.PropertyType.GetGenericArguments();
                var entityType = genericArguments[0];

                // create a resolve Func from the properties Delegate method
                Func<ResolveFieldContext<object>, object, object> resolve = (context, input) =>
                {
                    return ((Delegate)property.GetValue(this)).DynamicInvoke(context, input);
                };

                // create the field
                var field = this.CreateSimpleUpdateMutation(entityType, resolve, property.Name.ToFirstLower());
                this.SetAuthenticationAndAuthorizationConfigForField(field, property);
            }
        }

        /// <summary>
        /// Creates mutation fields for every property found that is annotated
        /// by the [Mutation] attribute and has the type of <see cref="Delete<,,>"/> Delegate.
        /// </summary>
        private void FindAndConvertPropertiesToDeleteMutations()
        {
            var properties = this.GetMutationPropertiesOfType(typeof(Delete<,,>));
            foreach (var property in properties)
            {
                // get the type arguments
                var genericArguments = property.PropertyType.GetGenericArguments();
                var idType = genericArguments[0];
                var entityType = genericArguments[1];
                var viewerType = genericArguments[2];

                // create a resolve Func from the properties Delegate method
                // the resolver expects the context, the id and an UpdateInput as inputs
                Func<ResolveFieldContext<object>, object, object, object> resolve = (context, id, input) =>
                {
                    return ((Delegate)(property.GetValue(this))).GetMethodInfo().Invoke(this, new object[] { context, id, input });
                };

                // create the field
                var field = this.CreateDeleteMutation(idType, entityType, viewerType, resolve, property.Name.ToFirstLower());
                this.SetAuthenticationAndAuthorizationConfigForField(field, property);
            }
        }

        /// <summary>
        /// Gets all properties that are annotated by the [Mutation] attribute and
        /// whose Delegate type matches the given type.
        /// </summary>
        /// <param name="type">The Delegate type to search for.</param>
        /// <returns>All mutation properties with a given Delegate type.</returns>
        private IEnumerable<PropertyInfo> GetMutationPropertiesOfType(Type type)
        {
            var typeName = type.Name;

            var properties = this.GetPropertiesWithAttribute<MutationAttribute>();
            return properties.Where(p => p.PropertyType.Name.Equals(typeName));
        }

        /// <summary>
        /// Sets the authentication and authorization configuration for a given field and property.
        /// </summary>
        private void SetAuthenticationAndAuthorizationConfigForField(FieldType field, PropertyInfo property)
        {
            var isAuthenticationRequired = property.GetAttribute<MutationAttribute>().IsAuthenticationRequired;
            var requiredRoles = property.GetRequiredRoles();
            field.RequiresRoles(requiredRoles);
            field.RequiresAuthentication(isAuthenticationRequired);
        }
    }
}
