using GraphQL.Types;
using Nitrolize.Convenience.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nitrolize.Extensions
{
    public static class PropertyInfoExtensions
    {
        /// <summary>
        /// Gets the first occurence of an Attribute of a PropertyInfo of a given type T.
        /// </summary>
        /// <typeparam name="T">The type of the desired attribute.</typeparam>
        /// <param name="propertyInfo">The property info.</param>
        /// <returns>An instance of an Attribute on the PropertyInfo of Type T.</returns>
        public static T GetAttribute<T>(this PropertyInfo propertyInfo) where T : Attribute
        {
            return (T)propertyInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault();
        }

        /// <summary>
        /// Gets the query arguments for a property that are annotated as [Argument(...)].
        /// </summary>
        /// <param name="property"></param>
        /// <returns>A list of QueryArguments.</returns>
        public static List<QueryArgument> GetQueryArguments(this PropertyInfo property)
        {
            var arguments = new List<QueryArgument>();
            foreach (ArgumentAttribute argumentAttribute in property.GetCustomAttributes(typeof(ArgumentAttribute), false))
            {
                arguments.Add(new QueryArgument(argumentAttribute.ArgumentType) { Name = argumentAttribute.Name });
            }

            return arguments;
        }

        /// <summary>
        /// Gets the required roles for a property that are annotated as [RequiredRole(...)].
        /// </summary>
        /// <param name="property"></param>
        /// <returns>A list of required roles.</returns>
        public static string[] GetRequiredRoles(this PropertyInfo property)
        {
            var requiredRolesAttribute = property.GetAttribute<RequiredRolesAttribute>();
            if (requiredRolesAttribute != null)
            {
                return requiredRolesAttribute.RequiredRoles;
            }

            return null;
        }
    }
}
