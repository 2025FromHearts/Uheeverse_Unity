using System.Collections.Generic;

namespace System
{
    internal class List<T> : IEnumerable<object>
    {
        private Collections.Generic.List<string> list;

        public List(Collections.Generic.List<string> list)
        {
            this.list = list;
        }
    }
}