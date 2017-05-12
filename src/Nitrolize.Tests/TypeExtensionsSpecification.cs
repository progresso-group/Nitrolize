using Machine.Fakes;
using Machine.Specifications;
using System;
using System.Collections.Generic;
using Nitrolize.Extensions;
using System.Reflection;
using FluentAssertions;
using System.ComponentModel;
using GraphQL.Types;
using Nitrolize.Types;

namespace Nitrolize.Tests
{
    [Subject(typeof(Nitrolize.Extensions.TypeExtensions))]
    public class TypeExtensionsSpecification : WithSubject<Object>
    {
    }

    public class When_requesting_exact_properties : TypeExtensionsSpecification
    {
        public abstract class ExactPropertyClassBase
        {
            public int BaseClassProperty { get; set; }
        }

        public class ExactPropertyClass
        {
            public ExactPropertyClass() { }
            public string Property1 { get; set; }
            public double Property2 { get; set; }
        }

        protected static PropertyInfo[] Result;

        Because of = () =>
        {
            Result = typeof(ExactPropertyClass).GetExactProperies();
        };

        It should_return_correct_number_of_properties = () => Result.Length.Should().Be(2);

        It should_return_all_class_properties = () =>
        {
            Result.Should().Contain(p => p.Name == "Property1");
            Result.Should().Contain(p => p.Name == "Property2");
        };

        It should_not_return_any_base_class_properties = () => Result.Should().NotContain(p => p.Name == "BaseClassProperty");

    }

    public class When_converting_to_virtual_class : TypeExtensionsSpecification
    {
        public class GenericClass<T> where T : class
        {
        }

        public class ExampleClass
        {
        }

        protected static Type Result;

        Because of = () =>
        {
            Result = typeof(GenericClass<ExampleClass>).ConvertToVirtualType();
        };

        It should_have_set_a_correct_name = () => Result.Name.Should().Be("GenericClassExampleClass");

        It should_derive_from_original_type = () => Result.GetTypeInfo().IsSubclassOf(typeof(GenericClass<ExampleClass>)).Should().BeTrue();
    }

    public abstract class GettingIdSpecification : TypeExtensionsSpecification
    {
        public class ModelClass
        {
            public Guid Id { get; set; }
        }

        public class ModelClass2
        {
            public Guid ModelClass2Id { get; set; }
        }

        public class ModelClass3
        {
            public Guid WrongNamedOrNotExistentId { get; set; }
        }

        protected static ModelClass Model;
        protected static ModelClass2 Model2;
        protected static ModelClass3 Model3;

        Establish context = () =>
        {
            Model = new ModelClass { Id = Guid.NewGuid() };
            Model2 = new ModelClass2 { ModelClass2Id = Guid.NewGuid() };
            Model3 = new ModelClass3 { WrongNamedOrNotExistentId = Guid.NewGuid() };
        };
    }

    public class When_getting_id : GettingIdSpecification
    {
        protected static Guid Result;
        protected static Guid Result2;

        Because of = () =>
        {
            Result = typeof(ModelClass).GetId<Guid>(Model);
            Result2 = typeof(ModelClass2).GetId<Guid>(Model2);
        };

        It should_have_got_the_property_named_id = () => Result.Should().NotBeEmpty();

        It should_have_got_the_property_named_typename_id = () => Result2.Should().NotBeEmpty();
    }

    public class When_getting_id_with_invalid_id_type : GettingIdSpecification
    {
        protected static Exception exception;

        Because of = () => exception = Catch.Exception(() => typeof(ModelClass).GetId<int>(Model));

        It should_throw_exception = () => exception.Should().NotBeNull();

        It should_throw_cast_eception = () => exception.Message.Should().Be("The Id property of ModelClass is of type Guid and cannot be casted to Int32.");
    }

    public class When_getting_id_with_non_existend_id_property : GettingIdSpecification
    {
        protected static Exception exception;

        Because of = () => exception = Catch.Exception(() => typeof(ModelClass3).GetId<Guid>(Model));

        It should_throw_exception = () => exception.Should().NotBeNull();

        It should_throw_cast_eception = () => exception.Message.Should().Be("Could not find any id property on ModelClass3.");
    }

    public class When_converting_id_properties_to_string : TypeExtensionsSpecification
    {
        public class Model
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public List<Item> Items { get; set; }
            public Guid SomeItemId { get; set; }
        }

        public class Item
        {
            public Guid Id { get; set; }
            public string ItemName { get; set; }
        }

        public class InputModel : Model
        {
            public string ClientMutationId { get; set; }
        }

        protected static Type Result;
        protected static Type ResultWithoutId;

        Because of = () =>
        {
            Result = typeof(InputModel).ConvertToInputType();
            ResultWithoutId = typeof(InputModel).ConvertToInputType(removeIdProperty: true);
        };

        It should_ship_with_subclass_properties = () => Result.GetProperty("ClientMutationId").Should().NotBeNull();

        It should_have_an_id_property_of_type_string = () => Result.GetProperty("Id").PropertyType.Name.Should().Be("String");

        It should_have_converted_the_SomeItem_id = () => Result.GetProperty("SomeItemId").PropertyType.Name.Should().Be("String");

        It should_ship_with_list_properties = () => Result.GetProperty("Items").Should().NotBeNull();

        It should_convert_ids_of_list_items = () => Result.GetProperty("Items").PropertyType.GetGenericArguments()[0].GetProperty("Id").PropertyType.Name.Should().Be("String");

        It should_have_created_class_attribute = () => Result.GetTypeInfo().GetCustomAttributes().Should().NotBeEmpty();

        It should_have_stored_original_id_type_in_class_attribute = () => Result.GetTypeInfo().GetCustomAttribute<TypeDescriptionProviderAttribute>().TypeName.Should().Contain("Guid");

        It should_have_omitted_id_property = () => ResultWithoutId.GetProperty("Id").Should().BeNull();
    }

    public class When_checking_for_id_property : TypeExtensionsSpecification
    {
        public class Model
        {
            public int Id { get; set; }
        }

        protected static bool Result;

        Because of = () => Result = typeof(Model).HasId();

        It should_have_found_id = () => Result.Should().Be(true);
    }

    public class When_mapping_to_string_graph_type : TypeExtensionsSpecification
    {
        protected static Type Result;

        Because of = () => Result = typeof(string).MapToGraphType();

        It should_have_mapped_to_string_graph_type = () => Result.Should().Be(typeof(StringGraphType));
    }

    public class When_mapping_to_object_graph_type : TypeExtensionsSpecification
    {
        public class Model
        {
        }

        protected static Type Result;

        Because of = () => Result = typeof(Model).MapToGraphType();

        It should_have_mapped_to_string_graph_type = () => Result.Should().Be(typeof(ObjectType<Model>));
    }

    public class When_mapping_to_node_object_graph_type : TypeExtensionsSpecification
    {
        public class Model
        {
            public int Id { get; set; }
        }

        protected static Type Result;

        Because of = () => Result = typeof(Model).MapToGraphType();

        It should_have_mapped_to_string_graph_type = () => Result.Should().Be(typeof(NodeObjectType<Model>));
    }

    public class When_mapping_to_list_graph_type : TypeExtensionsSpecification
    {
        protected static Type Result;

        Because of = () => Result = typeof(List<string>).MapToGraphType();

        It should_have_mapped_to_string_graph_type = () => Result.Should().Be(typeof(ListGraphType<StringGraphType>));
    }
}
