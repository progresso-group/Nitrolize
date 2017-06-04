using System;

namespace Nitrolize.Tests.Integration.Schema
{
    public class Viewer
    {
        public Guid Id { get; set; }

        public Viewer()
        {
            this.Id = Guid.NewGuid();
        }
    }
}
