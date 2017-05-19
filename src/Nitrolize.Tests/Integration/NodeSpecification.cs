using FluentAssertions;
using GraphQL;
using GraphQL.Http;
using GraphQL.Instrumentation;
using GraphQL.Validation.Complexity;
using Machine.Fakes;
using Machine.Specifications;
using Nitrolize.Types.Node;
using System.Collections.Generic;
using TestSchema = Nitrolize.Tests.Integration.Schema;

namespace Nitrolize.Tests.Schema
{
    [Subject(typeof(NodeType))]
    public class NodeTypeSpecification : WithSubject<TestSchema.Query>
    {
        protected static IDocumentExecuter DocumentExecuter = new DocumentExecuter();
        protected static IDocumentWriter DocumentWriter = new DocumentWriter(true);
        protected static dynamic Result;

        protected static object Execute(string query, string id)
        {
            var result = DocumentExecuter.ExecuteAsync(_ =>
            {
                _.Schema = new TestSchema.Schema();
                _.Query = query;
                _.OperationName = null;
                _.Inputs = new Inputs(new Dictionary<string, object>() { {"id", id } });

                _.ComplexityConfiguration = new ComplexityConfiguration { MaxDepth = 15 };
                _.FieldMiddleware.Use<InstrumentFieldsMiddleware>();
                _.UserContext = null;
                _.ValidationRules = null;
            }).Await().AsTask.Result;

            return result.Data;
        }
    }

    public class When_querying_a_node_field : NodeTypeSpecification
    {
        protected static string Query = @"
            query EntityList_ViewerRelayQL($id_0:ID!) {
              node(id:$id_0) {
                id,
                __typename,
                ...F0
              }
            }
            fragment F0 on ViewerType {
              _entityListdSPVg:entityList {
                id,
                name
              },
              id
            }
        ";

        Because of = () => Result = Execute(Query, "VXNlciNmOTM2OGNlNC0wNjhkLTQxN2ItYmZiZi0wMDdkMzEyYTA4ZmM=");

        It should_return_a_property = () => {
            var name = (string)Result["viewer"]["entityA"]["name"];
            name.Should().Be("The Entity A");
        };
    }
}
