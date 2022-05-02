using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamAlpha.Source
{
    public class StateDefault<T>
    {
        public delegate bool TransitionCondition(int from);

        public TransitionCondition Condition = delegate { return true; };
        public Action<T> Handle = delegate { };
        public Action<T> OnStart = delegate { };
        public Action<T> OnEnd = delegate { };
    }
    public class StateDefault
    {
        public delegate bool TransitionCondition(int from);

        public TransitionCondition Condition = delegate { return true; };
        public Action Handle = delegate { };
        public Action OnStart = delegate { };
        public Action<int> OnEnd = delegate { };
    }
}
