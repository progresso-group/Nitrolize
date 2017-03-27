using FluentAssertions;
using Machine.Fakes;
using Machine.Specifications;
using Nitrolize.Extensions;
using Nitrolize.Types.Input;
using System;

namespace Nitrolize.Tests
{
    [Subject(typeof(InputObjectGraphTypeExtensions))]
    public class InputObjectGraphTypeExtensionsSpecification : WithSubject<Object>
    {
        public class TestGraphType : InputType<SomeModel>
        {
        }

        public class SomeModel
        {
            public string SomeProperty { get; set; }
        }

        protected static TestGraphType Result;
    }

    public class When_instantiating_an_input_type : InputObjectGraphTypeExtensionsSpecification
    {
        Because of = () => Result = new TestGraphType();

        It should_have_added_all_simple_fields_from_its_model = () => Result.Fields.Should().Contain(f => f.Name == "someProperty");
    }
}
