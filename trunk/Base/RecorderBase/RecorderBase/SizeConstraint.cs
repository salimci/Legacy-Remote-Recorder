using Natek.Helpers.Limit;

namespace Natek.Recorders.Remote
{
    public abstract class SizeConstraint<T>: Constraint<T>
    {
        public long Size { get; set; }
    }
}
