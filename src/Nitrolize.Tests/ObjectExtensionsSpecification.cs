using FluentAssertions;
using Machine.Fakes;
using Machine.Specifications;
using Nitrolize.Extensions;
using Nitrolize.Identification;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nitrolize.Tests
{
    [Subject(typeof(ObjectExtensions))]
    public class ObjectExtensionsSpecification : WithSubject<Object>
    {
    }

    public class When_cloning_an_opject : ObjectExtensionsSpecification
    {
        [TypeDescriptionProvider(typeof(Guid))]
        public class ModelA
        {
            public string Id { get; set; }
            public double DoubleValue { get; set; }
            public List<ItemA> Items { get; set; }
            public List<ItemA> NullItems { get; set; }

            private string privateField = "private field";
            public string PrivateField
            {
                get
                {
                    return this.privateField;
                }
            }

            private List<ItemA> fieldItems = new[] { new ItemA { Id = GlobalId.ToGlobalId("ItemB", "1338"), Value = 1338 } }.ToList();
            public List<ItemA> FieldItems
            {
                get
                {
                    return this.fieldItems;
                }
            }
        }

        [TypeDescriptionProvider(typeof(int))]
        public class ItemA
        {
            public string Id { get; set; }
            public int Value { get; set; }
        }

        public class ModelB
        {
            public Guid Id { get; set; }
            public double DoubleValue { get; set; }
            public List<ItemB> Items { get; set; }
            public List<ItemB> NullItems { get; set; }

            private string privateField = "empty";
            public string PrivateField
            {
                get
                {
                    return this.privateField;
                }
            }

            private List<ItemB> fieldItems = new List<ItemB>();
            public List<ItemB> FieldItems
            {
                get
                {
                    return fieldItems;
                }
            }
        }

        public class ItemB
        {
            public int Id { get; set; }
            public int Value { get; set; }
        }

        protected static ModelA A;

        Establish context = () => A = new ModelA
        {
            Id = GlobalId.ToGlobalId("ModelB", "0a25a77b-d43f-4744-8095-ff5567797082"),
            DoubleValue = 13.37,
            Items = new[] { new ItemA { Id = GlobalId.ToGlobalId("ItemB", "1337"), Value = 1337 } }.ToList()
        };

        protected static ModelB Result;

        Because of = () => Result = A.CloneAs<ModelB>();

        It should_have_converted_simple_properties = () => Result.DoubleValue.Should().Be(13.37);

        It should_have_converted_id_property = () => Result.Id.Should().Be(Guid.Parse("0a25a77b-d43f-4744-8095-ff5567797082"));

        It should_have_cloned_list_properties = () => Result.Items.Should().NotBeNull();

        It should_have_cloned_list_items = () => Result.Items.Should().NotBeEmpty();

        It should_have_cloned_properties_of_list_items = () => Result.Items[0].Value.Should().Be(1337);

        It should_have_skipped_null_lists = () => Result.NullItems.Should().BeNull();

        It should_have_cloned_fields = () => Result.PrivateField.Should().Be("private field");

        It should_have_cloned_field_lists = () => Result.FieldItems.Should().NotBeNull();
    }
}
