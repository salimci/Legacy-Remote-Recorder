using Natek.Helpers.Execution;

namespace Natek.Helpers.Limit
{
    public abstract class Constraint<T>
    {
        public abstract NextInstruction Apply(T target, object context);
    }
}
