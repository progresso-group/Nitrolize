using Nitrolize.Identification;
using System;

namespace Nitrolize.Tests.Schema
{
    public class Viewer
    {
        public string Id { get; set; }

        public Viewer()
        {
            this.Id = GlobalId.ToGlobalId("Viewer", Guid.NewGuid().ToString());
        }
    }
}
