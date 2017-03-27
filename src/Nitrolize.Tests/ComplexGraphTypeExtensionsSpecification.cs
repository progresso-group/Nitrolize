using FluentAssertions;
using GraphQL.Types;
using Machine.Fakes;
using Machine.Specifications;
using Nitrolize.Extensions;
using Nitrolize.Types;
using System;

namespace Nitrolize.Tests
{
    [Subject(typeof(ComplexGraphTypeExtensions))]
    public class ComplexGraphTypeExtensionsSpecification : WithSubject<Object>
    {
        public class TestGraphType : ObjectGraphType
        {
        }

        public class SomeModel
        {
            public Guid Id { get; set; }
        }

        protected static TestGraphType Result;

        Establish context = () => Result = new TestGraphType();
    }

    public class When_adding_a_single_field : ComplexGraphTypeExtensionsSpecification
    {
        Because of = () => Result.AddSingleField<SomeModel, NodeObjectType<SomeModel>>(context => null);

        It should_have_added_the_single_field = () => Result.Fields.Should().Contain(f => f.Name == "someModel");
    }

    public class When_adding_a_list_field : ComplexGraphTypeExtensionsSpecification
    {
        Because of = () => Result.AddListField<SomeModel, NodeObjectType<SomeModel>>();

        It should_have_added_the_list_field = () => Result.Fields.Should().Contain(f => f.Name == "someModels");
    }

    public class When_adding_a_connection_field : ComplexGraphTypeExtensionsSpecification
    {
        Because of = () => Result.AddConnectionField<SomeModel, NodeObjectType<SomeModel>>(context => null);

        It should_have_added_the_list_field = () => Result.Fields.Should().Contain(f => f.Name == "someModels");
    }
}
