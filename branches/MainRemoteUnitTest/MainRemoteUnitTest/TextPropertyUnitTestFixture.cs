using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    /// <summary>
    /// Summary description for TextPropertyUnitTestFixture
    /// </summary>
    [TestFixture]
    public class TextPropertyUnitTestFixture
    {

// ReSharper disable InconsistentNaming

        readonly TextProperty text_property = new TextProperty();

// ReSharper restore InconsistentNaming

        /// <summary>
        /// Method Name : GetValue
        ///
        ///Method Description :  The method check the target and property info which is getvalue parameter 
        ///
        ///Test Scenario :  If target and context are null
        ///
        ///Known Input :
        ///    * object target = null
        ///    * object context = null
        ///    * text_property.PropertyInfo = null
        ///    
        ///
        ///Expected Output :
        ///    Return should null
        /// </summary>
        /// 
        [Test(Description = "If target and context are null")]
        public void GetValue_IfTargetAndContextAreNull_ReturnNull()
        {
            //Arrange
            object target = null;
            object context = null;
            text_property.PropertyInfo = null;
            //Act
// ReSharper disable once ExpressionIsAlwaysNull
// ReSharper disable ExpressionIsAlwaysNull
            var actual = text_property.GetValue(target, context);
// ReSharper restore ExpressionIsAlwaysNull
            //Assert
            Assert.AreEqual(actual,null);
        }

        /// <summary>
        /// Method Name : GetValue
        ///
        ///Method Description :  The method check the target and property info which is getvalue parameter
        ///
        ///Test Scenario :  If target and context is not null
        ///
        ///Known Input :
        ///    * object target = 123
        ///    * object context = "ipsum"
        ///    
        ///
        ///Expected Output :
        ///    * Return should value
        /// </summary>
        /// 
        [Test(Description = "If Parameter Is Not Null")]
        public void GetValue_IfParameterIsNotNull_ReturnValue()
        {
            //Arrange
            object target = 123;
            object context = "ipsum";
          
            //Act
            var actual = text_property.GetValue(target, context);
            //Assert
            Assert.AreEqual(actual, null);
        }

        /// <summary>
        /// Method Name : SetValue
        ///
        ///Method Description :  The method check the target and property info which is setvalue parameter
        ///
        ///Test Scenario :  If target and context are null
        ///
        ///Known Input :
        ///    * object target = null
        ///    * object context = null
        ///    * string value = null
        ///    
        ///
        ///Expected Output :
        ///    *No  Return 
        /// </summary>
        /// 
        [Test(Description = "If target and context are null")]
        public void SetValue_IfTargetAndContextAreNull_NoReturn()
        {
            //Arrange
            object target = null;
            object context = null;
            string value = null;
            //Act
// ReSharper disable once ExpressionIsAlwaysNull
// ReSharper disable ExpressionIsAlwaysNull
             text_property.SetValue(target, context, value);
// ReSharper restore ExpressionIsAlwaysNull
            //Assert
            
        }

        /// <summary>
        /// Method Name : SetValue
        ///
        ///Method Description :  The method check the target and property info which is getvalue parameter
        ///
        ///Test Scenario :  If target and context is not null
        ///
        ///Known Input :
        ///     * object target = "lorem ipsum"
        ///     * object context = "lorem ipsum"
        ///     * const string value = "lorem ipsum"
        ///    
        ///
        ///Expected Output :
        ///     * No Return
        /// </summary>
        ///     
        [Test(Description = "If Parameter Is Not Null")]
        public void SetValue_IfParameterIsNotNull_NoReturn()
        {
            //Arrange
            object target = "lorem ipsum";
            object context = "lorem ipsum";
            const string value = "lorem ipsum";
           //Act
            text_property.SetValue(target, context, value);
            //Assert
        }
    }
}
