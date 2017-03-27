using FluentAssertions;
using Machine.Fakes;
using Machine.Specifications;
using Nitrolize.Extensions;
using Nitrolize.Types;
using System;

namespace Nitrolize.Tests
{
    [Subject(typeof(ObjectGraphTypeExtensions))]
    public class ObjectGraphTypeExtensionsSpecification : WithSubject<Object>
    {
        public class TestGraphType : NodeObjectType<SomeModel>
        {
        }

        public class SomeModel
        {
            public bool BoolProperty { get; set; }
            public Guid GuidProperty { get; set; }
            public string StringProperty { get; set; }
            public int IntProperty { get; set; }
            public decimal DecimalProperty { get; set; }
            public float FloatProperty { get; set; }
            public double DoubleProperty { get; set; }
            public DateTime DateTimeProperty { get; set; }
        }

        protected static TestGraphType Result;
    }

    public class When_instantiating_an_object_type : ObjectGraphTypeExtensionsSpecification
    {
        Because of = () => Result = new TestGraphType();

        It should_have_added_all_primitive_fields_from_its_model = () =>
        {
            Result.Fields.Should().Contain(f => f.Name == "boolProperty");
            Result.Fields.Should().Contain(f => f.Name == "guidProperty");
            Result.Fields.Should().Contain(f => f.Name == "stringProperty");
            Result.Fields.Should().Contain(f => f.Name == "intProperty");
            Result.Fields.Should().Contain(f => f.Name == "decimalProperty");
            Result.Fields.Should().Contain(f => f.Name == "floatProperty");
            Result.Fields.Should().Contain(f => f.Name == "doubleProperty");
            Result.Fields.Should().Contain(f => f.Name == "dateTimeProperty");
        };
    }
}
