using FluentAssertions;
using Machine.Fakes;
using Machine.Specifications;
using Nitrolize.Types;
using System.Linq;

namespace Nitrolize.Tests
{
    public enum ExampleEnum
    {
        ExampleValue = 10,
        EliteValue = 1337,
        FailValue = 1338
    }

    [Subject(typeof(EnumerationType<ExampleEnum>))]
    public class EnumerationTypeSpecification : WithSubject<EnumerationType<ExampleEnum>>
    {
    }

    public class When_creating_the_type : EnumerationTypeSpecification
    {
        protected static EnumerationType<ExampleEnum> Result;

        Because of = () =>
        {
            Result = new EnumerationType<ExampleEnum>();
        };

        It should_set_correct_name = () => Result.Name.Should().Be("ExampleEnum");

        It should_create_all_values = () =>
        {
            Result.Values.ToList()[0].Name.ShouldBeEquivalentTo("ExampleValue");
            Result.Values.ToList()[0].Value.ShouldBeEquivalentTo(10);

            Result.Values.ToList()[1].Name.ShouldBeEquivalentTo("EliteValue");
            Result.Values.ToList()[1].Value.ShouldBeEquivalentTo(1337);

            Result.Values.ToList()[2].Name.ShouldBeEquivalentTo("FailValue");
            Result.Values.ToList()[2].Value.ShouldBeEquivalentTo(1338);
        };
    }
}
