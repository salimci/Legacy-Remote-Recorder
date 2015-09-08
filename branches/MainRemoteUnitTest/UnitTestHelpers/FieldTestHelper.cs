using System.Reflection;

namespace Natek.Recorders.Remote.Test.UnitTestHelper
{
    public class FieldTestHelper
    {
        static public bool SetInstanceFieldValue(string fieldName, object objectInstance, object fieldValue)
        {

            const BindingFlags bindingFlag = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            var typeOfObject = objectInstance.GetType();

            var fieldInfo = typeOfObject.GetField(fieldName, bindingFlag);

            if (fieldInfo == null) return false;
            fieldInfo.SetValue(objectInstance, fieldValue);
            return true;
        }

        static public object GetInstanceFieldValue(string fieldName, object objectInstance)
        {

            const BindingFlags bindingFlag = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            var typeOfObject = objectInstance.GetType();

            var fieldInfo = typeOfObject.GetField(fieldName, bindingFlag);

            return fieldInfo == null ? null : fieldInfo.GetValue(objectInstance);
        }
    }
}
