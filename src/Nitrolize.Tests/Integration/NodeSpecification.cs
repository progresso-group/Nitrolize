using FluentAssertions;
using GraphQL;
using GraphQL.Http;
using GraphQL.Instrumentation;
using GraphQL.Validation.Complexity;
using Machine.Fakes;
using Machine.Specifications;
using Nitrolize.Tests.Integration.Authentication;
using Nitrolize.Types.Node;
using Nitrolize.Validation;
using System.Collections.Generic;
using System.Linq;
using TestSchema = Nitrolize.Tests.Integration.Schema;

namespace Nitrolize.Tests.Schema
{
    [Subject(typeof(NodeType))]
    public class NodeTypeSpecification : WithSubject<TestSchema.Query>
    {
        protected static IDocumentExecuter DocumentExecuter = new DocumentExecuter();
        protected static IDocumentWriter DocumentWriter = new DocumentWriter(true);
        protected static ExecutionResult Result;

        protected static ExecutionResult Execute(string query, string id)
        {
            var result = DocumentExecuter.ExecuteAsync(_ =>
            {
                _.Schema = new TestSchema.Schema();
                _.Query = query;
                _.OperationName = null;
                _.Inputs = new Inputs(new Dictionary<string, object>() { { "id_0", id } });

                _.ComplexityConfiguration = new ComplexityConfiguration { MaxDepth = 15 };
                _.FieldMiddleware.Use<InstrumentFieldsMiddleware>();
                _.UserContext = new MockUserContext();
                _.ValidationRules = new[] { new RequiresAuthValidationRule<int>() };
            }).Await().AsTask.Result;

            if (result.Errors != null && result.Errors.Count > 0)
            {
                throw new System.Exception(result.Errors.ToList().First().Message);
            }

            return result;
        }
    }
    
    public class When_querying_the_node_field_for_an_entity : NodeTypeSpecification
    {
        protected static string Query = @"
            query Entity($id_0:ID!) {
              node(id:$id_0) {
                id
                __typename
                ...F0
              }
            }
            fragment F0 on EntityA {
              id
              name
            }
        ";

        Because of = () => Result = Execute(Query, "RW50aXR5QSMwMzJhMTViMy1kN2I4LTQxMWMtYWE4YS0yNDMyOTY4N2ExNjI=");

        It should_return_a_property = () => {
            var data = (dynamic)Result.Data;
            var name = (string)data["node"]["name"];
            name.Should().Be("EntityA from node");
        };
    }

    public class When_querying_the_node_field_for_list : NodeTypeSpecification
    {
        protected static string Query = @"
            query EntityList_ViewerRelayQL($id_0:ID!) {
              node(id:$id_0) {
                id
                __typename
                ...F0
              }
            }
            fragment F0 on ViewerType {
              _entityListdSPVg:entityList {
                id
                name
              }
              id
            }
        ";

        Because of = () => Result = Execute(Query, "Vmlld2VyIzNjMmE3YzRjLTllYTktNDBlNi1iNzRmLTFhOTlkYjI5ODQzMg==");

        It should_return_a_property = () => {
            var data = (dynamic)Result.Data;
            var name = (string)data["node"]["_entityListdSPVg"][0]["name"];
            name.Should().Be("No1");
        };
    }
}
