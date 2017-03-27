using FluentAssertions;
using Machine.Fakes;
using Machine.Specifications;
using Nitrolize.Extensions;

namespace Nitrolize.Tests
{
    [Subject(typeof(StringExtensions))]
    public class StringExtensionsSpecification : WithSubject<string>
    {
    }

    public class When_converting_to_first_lower : StringExtensionsSpecification
    {
        protected static string Result;

        Establish context = () => Result = "SomeExampleString";

        Because of = () => Result = Result.ToFirstLower();

        It should_have_set_first_character_to_lower_case = () => Result[0].Should().Be('s');
    }
}
