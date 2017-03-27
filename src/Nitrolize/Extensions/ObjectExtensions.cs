using Nitrolize.Identification;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Nitrolize.Extensions
{
    public static class ObjectExtensions
    {
        public static object CloneAs(this object subject, Type t)
        {
            var cloneAsMethod = typeof(ObjectExtensions).GetMethods().First(m => m.IsGenericMethod);
            cloneAsMethod = cloneAsMethod.MakeGenericMethod(t);

            return cloneAsMethod.Invoke(null, new object[] { subject, false });
        }

        public static T CloneAs<T>(this object subject, bool omitIdProperty = false) where T : class, new()
        {
            var clone = new T();

            clone = CloneFields<T>(subject, clone);
            clone = CloneProperties<T>(subject, clone, omitIdProperty);

            return clone;
        }

        private static T CloneFields<T>(object subject, T clone)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var cloneType = clone.GetType();

            foreach (var field in subject.GetType().GetFields(bindingFlags))
            {

                // skip baking fields
                if (field.Name.EndsWith("BackingField"))
                {
                    continue;
                }

                var fieldValue = field.GetValue(subject);
                var cloneField = cloneType.GetField(field.Name, bindingFlags);

                if (field.FieldType.GetInterface("ICollection") != null && field.FieldType.IsGenericType)
                {
                    var collectionType = field.FieldType.GetGenericTypeDefinition();
                    var parameterType = field.FieldType.GenericTypeArguments[0];

                    var cloneCollectionType = cloneField.FieldType.GetGenericTypeDefinition();
                    var cloneParameterType = cloneField.FieldType.GenericTypeArguments[0];

                    var collection = (IList)fieldValue;
                    IList cloneCollection = null;

                    if (collection != null)
                    {
                        cloneCollection = (IList)Activator.CreateInstance(cloneField.FieldType);

                        foreach (var item in collection)
                        {
                            cloneCollection.Add(item.CloneAs(cloneParameterType));
                        }
                    }

                    cloneField.SetValue(clone, cloneCollection);

                    continue;
                }

                cloneField.SetValue(clone, fieldValue);
            }

            return clone;
        }

        private static T CloneProperties<T>(object subject, T clone, bool omitIdProperty)
        {
            var cloneType = clone.GetType();

            foreach (var property in subject.GetType().GetProperties())
            {
                // skip properties that do not have setters
                if (property.GetSetMethod() == null)
                {
                    continue;
                }

                var propertyValue = property.GetValue(subject, null);
                var cloneProperty = cloneType.GetProperty(property.Name);

                // handle id
                if (property.Name == "Id" && !omitIdProperty)
                {
                    var attribute = subject.GetType().GetCustomAttribute<TypeDescriptionProviderAttribute>();
                    var idTypeName = attribute.TypeName;

                    if (idTypeName.Contains("Guid"))
                    {
                        var idValue = Guid.Empty;

                        try
                        {
                            idValue = GlobalId.ToLocalId<Guid>((string)propertyValue);
                        }
                        catch
                        {
                            idValue = Guid.Empty;
                        }

                        cloneProperty.SetValue(clone, idValue);
                    }

                    if (idTypeName.Contains("Int32"))
                    {
                        var idValue = GlobalId.ToLocalId<int>((string)propertyValue);
                        cloneProperty.SetValue(clone, idValue);
                    }

                    continue;
                }

                // handle foreign keys
                if (property.Name.EndsWith("Id"))
                {
                    try
                    {
                        cloneProperty.SetValue(clone, propertyValue);
                    }
                    catch
                    {
                        var targetTypeName = cloneProperty.PropertyType.Name;

                        if (targetTypeName.Contains("Guid"))
                        {
                            var value = Guid.Empty;

                            try
                            {
                                value = GlobalId.ToLocalId<Guid>((string)propertyValue);
                            }
                            catch
                            {
                                value = Guid.Empty;
                            }

                            cloneProperty.SetValue(clone, value);
                        }

                        if (targetTypeName.Contains("Int32"))
                        {
                            var value = GlobalId.ToLocalId<int>((string)propertyValue);
                            cloneProperty.SetValue(clone, value);
                        }

                        continue;
                    }
                }

                // handle collections
                if (property.PropertyType.GetInterface("ICollection") != null && property.PropertyType.IsGenericType)
                {
                    // skip properties without setters
                    if (cloneProperty.GetSetMethod() == null)
                    {
                        continue;
                    }

                    var collectionType = property.PropertyType.GetGenericTypeDefinition();
                    var parameterType = property.PropertyType.GenericTypeArguments[0];

                    var cloneCollectionType = cloneProperty.PropertyType.GetGenericTypeDefinition();
                    var cloneParameterType = cloneProperty.PropertyType.GenericTypeArguments[0];

                    var collection = (IList)propertyValue;
                    IList cloneCollection = null;

                    if (collection != null)
                    {
                        cloneCollection = (IList)Activator.CreateInstance(cloneProperty.PropertyType);

                        foreach (var item in collection)
                        {
                            cloneCollection.Add(item.CloneAs(cloneParameterType));
                        }
                    }

                    cloneProperty.SetValue(clone, cloneCollection);

                    continue;
                }

                // handle all other properties
                cloneProperty.SetValue(clone, propertyValue);
            }

            return clone;
        }

        public static IEnumerable<PropertyInfo> GetPropertiesWithAttribute<TAttribute>(this Object subject) where TAttribute : Attribute
        {
            return subject.GetType().GetProperties().Where(p => p.GetCustomAttributes(typeof(TAttribute), false).Any());
        }
    }
}
