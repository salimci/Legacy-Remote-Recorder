using Natek.Helpers.Execution;

namespace Natek.Recorders.Remote
{
    public class TextSizeConstraint<T> : SizeConstraint<T> where T:RecWrapper
    {
        public TextProperty Property { get; set; }
        public override NextInstruction Apply(T target, object context)
        {
            if (Property != null)
            {
                var value = Property.GetValue(target, context);
                if (value != null && value.Length > Size)
                {
                    Property.SetValue(target,context,value.Substring(0,(int)Size));
                }
            } 
            return NextInstruction.Do;
        }
    }
}
