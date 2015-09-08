namespace Natek.Helpers.CSharp
{
    public static class ArrayHelper
    {
        public static bool AssureLength<T>(T[] arr, int minLength = 0, bool allowNull = false)
        {
            if (arr == null)
                return allowNull;
            return arr.Length >= minLength;
        }
    }
}
