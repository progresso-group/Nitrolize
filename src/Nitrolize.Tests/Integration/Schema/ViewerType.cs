using Nitrolize.Convenience.Attributes;
using Nitrolize.Convenience.Delegates;
using Nitrolize.Models;
using Nitrolize.Tests.Integration.Models;
using Nitrolize.Types.Base;
using System;
using System.Collections.Generic;

namespace Nitrolize.Tests.Integration.Schema
{
    public class ViewerType : ViewerTypeBase
    {
        [Field(IsAuthenticationRequired = false)]
        public Field<EntityA, Guid> EntityA => (context, id) =>
        {
            var entity = new EntityA
            {
                Id = new Guid(),
                Name = "The Entity A",
                Entities = new List<EntityB>()
                    {
                        new EntityB { Id = new Guid(), Value = 1.1 },
                        new EntityB { Id = new Guid(), Value = 2.2 }
                    }
            };
            return entity;
        };

        [List(IsAuthenticationRequired = false)]
        public ListField<EntityA> EntityList => (context) =>
        {
            var list = new List<EntityA>()
                    {
                        new EntityA { Id = new Guid(), Name = "No1" },
                        new EntityA { Id = new Guid(), Name = "No2" }
                    };

            return list;
        };

        [Connection(IsAuthenticationRequired = false)]
        public ConnectionField<EntityA> EntityConnection => (context, parameters) =>
        {
            var list = new List<EntityA>()
                    {
                        new EntityA { Id = new Guid(), Name = "No1" },
                        new EntityA { Id = new Guid(), Name = "No2" }
                    };
            var connection = new Connection<EntityA, Guid>(list);

            connection.PageInfo.HasPreviousPage = false;
            connection.PageInfo.HasNextPage = true;

            return connection;
        };
    }
}
