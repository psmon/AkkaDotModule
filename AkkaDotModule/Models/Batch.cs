using System.Collections.Generic;
using System.Collections.Immutable;

namespace AkkaDotModule.Models
{
    public class BatchData
    {
        public object Data { get; set; }
    }

    public class BatchList
    {
        public BatchList(ImmutableList<object> obj)
        {
            Obj = obj;
        }

        public ImmutableList<object> Obj { get; }
    }
}
