using FluentAssertions;
using GraphQL;
using GraphQL.Http;
using GraphQL.Instrumentation;
using GraphQL.Validation.Complexity;
using Machine.Fakes;
using Machine.Specifications;
using Nitrolize.Tests.Integration.Authentication;
using Nitrolize.Types.Base;
using Nitrolize.Validation;
using System.Linq;
using TestSchema = Nitrolize.Tests.Integration.Schema;

namespace Nitrolize.Tests.Schema
{
    [Subject(typeof(ViewerTypeBase))]
    public class ViewerTypeSpecification : WithSubject<TestSchema.ViewerType>
    {
        protected static IDocumentExecuter DocumentExecuter = new DocumentExecuter();
        protected static IDocumentWriter DocumentWriter = new DocumentWriter(true);
        protected static dynamic Result;

        protected static object Execute(string query, string inputsString = null)
        {
            var inputs = new Inputs();

            if (inputsString != null)
            {
                inputs = inputsString.ToInputs();
            }

            var result = DocumentExecuter.ExecuteAsync(_ =>
            {
                _.Schema = new TestSchema.Schema();
                _.Query = query;
                _.OperationName = null;
                _.Inputs = inputs;

                _.ComplexityConfiguration = new ComplexityConfiguration { MaxDepth = 15 };
                _.FieldMiddleware.Use<InstrumentFieldsMiddleware>();
                _.UserContext = new MockUserContext();
                _.ValidationRules = new[] { new RequiresAuthValidationRule<int>() };
            }).Await().AsTask.Result;

            if (result.Errors != null && result.Errors.Count > 0)
            {
                throw new System.Exception(result.Errors.ToList().First().Message);
            }

            return result.Data;
        }
    }

    public class When_querying_a_field : ViewerTypeSpecification
    {
        protected static string Query = @"
            {
                viewer {
                    entityA(id: ""VXNlciNmOTM2OGNlNC0wNjhkLTQxN2ItYmZiZi0wMDdkMzEyYTA4ZmM="") {
                        id
                        name
                    }
                }
            }";

        Because of = () => Result = Execute(Query);

        It should_return_a_property = () => {
            var name = (string)Result["viewer"]["entityA"]["name"];
            name.Should().Be("The Entity A");
        };
    }

    public class When_querying_a_field_that_provides_a_list : ViewerTypeSpecification
    {
        protected static string Query = @"
            {
                viewer {
                    entityA(id: ""VXNlciNmOTM2OGNlNC0wNjhkLTQxN2ItYmZiZi0wMDdkMzEyYTA4ZmM="") {
                        id
                        name
                        entities {
                            edges {
                                node {
                                    id
                                    value
                                }
                            }
                        }
                    }
                }
            }";

        Because of = () => Result = Execute(Query);

        It should_return_a_property_of_an_item_in_the_list = () => {
            var value = (double)Result["viewer"]["entityA"]["entities"]["edges"][0]["node"]["value"];
            value.Should().Be(1.1);
        };
    }

    public class When_querying_a_list : ViewerTypeSpecification
    {
        protected static string Query = @"
            {
                viewer {
                    entityList {
                        id
                        name
                    }
                }
            }
        ";

        Because of = () => Result = Execute(Query);

        It should_return_items = () => ((object)Result["viewer"]["entityList"]).Should().NotBeNull();

        It should_return_first_item_name = () => ((object)Result["viewer"]["entityList"][0]["name"]).Should().Be("No1");

        It should_return_second_item_name = () => ((object)Result["viewer"]["entityList"][1]["name"]).Should().Be("No2");
    }

    public class When_querying_a_connection : ViewerTypeSpecification
    {
        protected static string Query = @"
            {
                viewer {
                    entityConnection(first: 100) {
                        edges {
                            cursor
                            node {
                                id
                                name
                            }
                        }
                        pageInfo {
                            startCursor
                            endCursor
                            hasPreviousPage
                            hasNextPage
                        }
                    }
                }
            }
        ";

        Because of = () => Result = Execute(Query);

        It should_return_page_info = () => ((object)Result["viewer"]["entityConnection"]["pageInfo"]).Should().NotBeNull();

        It should_return_page_info_start_cursor = () => ((object)Result["viewer"]["entityConnection"]["pageInfo"]["startCursor"]).Should().NotBeNull();

        It should_return_page_info_end_cursor = () => ((object)Result["viewer"]["entityConnection"]["pageInfo"]["endCursor"]).Should().NotBeNull();

        It should_return_page_info_hasPreviousPage = () => ((object)Result["viewer"]["entityConnection"]["pageInfo"]["hasPreviousPage"]).Should().Be(false);

        It should_return_page_info_hasNextPage = () => ((object)Result["viewer"]["entityConnection"]["pageInfo"]["hasNextPage"]).Should().Be(true);

        It should_return_edges = () => ((object)Result["viewer"]["entityConnection"]["edges"]).Should().NotBeNull();

        It should_return_first_edge_cursor = () => ((object)Result["viewer"]["entityConnection"]["edges"][0]["cursor"]).Should().NotBeNull();

        It should_return_first_edge_node_name = () => ((object)Result["viewer"]["entityConnection"]["edges"][0]["node"]["name"]).Should().Be("No1");

        It should_return_second_edge_cursor = () => ((object)Result["viewer"]["entityConnection"]["edges"][1]["cursor"]).Should().NotBeNull();

        It should_return_second_edge_node_name = () => ((object)Result["viewer"]["entityConnection"]["edges"][1]["node"]["name"]).Should().Be("No2");
    }

    public class When_calling_a_simple_update_mutation : ViewerTypeSpecification
    {
        protected static string Query = @"
                mutation simpleUpdate($input_0: updateEntityAInput!) {
                    updateEntityA(input: $input_0) {
                        id
                        name
                    }
                }            
        ";

        protected static string Variables = @"
            {
                input_0: {
                    id: ""VXNlciNmOTM2OGNlNC0wNjhkLTQxN2ItYmZiZi0wMDdkMzEyYTA4ZmM="",
                    name: ""example entity"",
                    entities: []
                }
            }
        ";

        Because of = () => Result = Execute(Query, Variables);

        It should_return_the_changed_object = () => ((object)Result["updateEntityA"]["name"]).Should().Be("example entity_mutated");
    }
}
