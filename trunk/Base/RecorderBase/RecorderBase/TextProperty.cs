using System.Reflection;
using Natek.Helpers.Reflection;

namespace Natek.Recorders.Remote
{
    public class TextProperty:IProperty<string>
    {
        public PropertyInfo PropertyInfo { get; set; }

        public string GetValue(object target, object context)
        {
            return (target == null || PropertyInfo == null)
                ? null
                : PropertyInfo.GetValue(target, context as object[]) as string;
        }

        public void SetValue(object target, object context, string value)
        {
            if(target != null && PropertyInfo != null)
                PropertyInfo.SetValue(target,value,context as object[]);
        }
    }
}
