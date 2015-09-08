namespace Natek.Helpers.Reflection
{
    public interface IProperty<T>
    {
        T GetValue(object target, object context);
         void SetValue(object target, object context, T value);
    }
}
