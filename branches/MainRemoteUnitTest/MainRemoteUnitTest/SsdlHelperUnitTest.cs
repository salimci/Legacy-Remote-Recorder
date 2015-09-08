using System;
using System.Text;
using Natek.Helpers.Security.AccessControl;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class SsdlHelperUnitTest
    {
        /// <summary>
        /// Method Name : DecodeSsdl
        /// 
        /// Method Desciption : Decoded ssdl codes
        /// 
        /// Test Scenerio : If ssdl code is empty
        /// 
        /// Known Input :
        ///     * ssdlStr = String.Empty
        ///     * decodeBuffer = null
        ///     * domainBuffer = null
        ///     * usernameBuffer = null
        ///     * formatted = 0
        /// 
        /// Expected Output : 
        ///     * String.Empty should return
        /// </summary>
        [Test(Description = "If ssdl code is empty")]
        public void DecodeSsdl_IfSsdlisEmpty_ReturnStringEmpty()
        {
            //Act
            var ssdlStr = String.Empty;
            StringBuilder decodeBuffer = null;
            StringBuilder domainBuffer = null;
            StringBuilder usernameBuffer = null;
            var formatted = 0;

            //Arrange
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = SsdlHelper.DecodeSsdl(ssdlStr, decodeBuffer, domainBuffer, usernameBuffer,ref formatted);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
        }

        /// <summary>
        /// Method Name : DecodeSsdl
        /// 
        /// Method Desciption : Decoded ssdl codes
        /// 
        /// Test Scenerio : If ssdl code is not empty and has valid value, other parameters are null
        /// 
        /// Known Input :
        ///     * ssdlStr = O:BAG:SYD:(D;;0xf0007;;;AN)(D;;0xf0007;;;BG)(A;;0xf0005;;;SY)(A;;0x5;;;BA)
        ///     * decodeBuffer = null
        ///     * domainBuffer = null
        ///     * usernameBuffer = null
        ///     * formatted = 0
        /// 
        /// Expected Output : 
        ///     * O:BAG:SYD:(D;;0xf0007;;;AN)(D;;0xf0007;;;BG)(A;;0xf0005;;;SY)(A;;0x5;;;BA) should return
        /// </summary>
        [Test(Description = "If ssdl code is not empty and has valid value, other parameters are null")]
        public void DecodeSsdl_IfSsdlisNotEmptyAnd_OtherParametersNull_ReturnString()
        {
            //Act
            const string ssdlStr = "O:BAG:SYD:(D;;0xf0007;;;AN)(D;;0xf0007;;;BG)(A;;0xf0005;;;SY)(A;;0x5;;;BA)";
            StringBuilder decodeBuffer = null;
            StringBuilder domainBuffer = null;
            StringBuilder usernameBuffer = null;
            var formatted = 0;

            //Arrange
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = SsdlHelper.DecodeSsdl(ssdlStr, decodeBuffer, domainBuffer, usernameBuffer, ref formatted);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, ssdlStr);
        }

        /// <summary>
        /// Method Name : DecodeSsdl
        /// 
        /// Method Desciption : Decoded ssdl codes
        /// 
        /// Test Scenerio : If decodebuffer parameter is not null
        /// 
        /// Known Input :
        ///     * ssdlStr = O:BAG:SYD:(D;;0xf0007;;;AN)(D;;0xf0007;;;BG)(A;;0xf0005;;;SY)(A;;0x5;;;BA)
        ///     * decodeBuffer = new StringBuilder()
        ///     * domainBuffer = null
        ///     * usernameBuffer = null
        ///     * formatted = 0
        /// 
        /// Expected Output : 
        ///     * ControlFlags=DiscretionaryAclPresent, SelfRelative; ResourceManagerControlBits=0; Group=S-1-5-18; Owner=S-1-5-32-544; DiscretionaryAcl(AceType=AccessDenied; AceFlags=None; AuditFlags=None; InheritnaceFlags=None; IsInherited=False; PropagationFlags=None; User=S-1-5-7); DiscretionaryAcl(AceType=AccessDenied; AceFlags=None; AuditFlags=None; InheritnaceFlags=None; IsInherited=False; PropagationFlags=None; User=S-1-5-32-546); DiscretionaryAcl(AceType=AccessAllowed; AceFlags=None; AuditFlags=None; InheritnaceFlags=None; IsInherited=False; PropagationFlags=None; User=S-1-5-18); DiscretionaryAcl(AceType=AccessAllowed; AceFlags=None; AuditFlags=None; InheritnaceFlags=None; IsInherited=False; PropagationFlags=None; User=S-1-5-32-544)
        /// </summary>
        [Test(Description = "If ssdl code is not empty and has valid value, other parameters are null")]
        public void DecodeSsdl_IfDecodeBufferIsNotNull_ReturnString()
        {
            //Act
            const string ssdlStr = "O:BAG:SYD:(D;;0xf0007;;;AN)(D;;0xf0007;;;BG)(A;;0xf0005;;;SY)(A;;0x5;;;BA)";
            var decodeBuffer = new StringBuilder();
            StringBuilder domainBuffer = null;
            StringBuilder usernameBuffer = null;
            var formatted = 0;

            //Arrange
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = SsdlHelper.DecodeSsdl(ssdlStr, decodeBuffer, domainBuffer, usernameBuffer, ref formatted);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "ControlFlags=DiscretionaryAclPresent, SelfRelative; ResourceManagerControlBits=0; Group=S-1-5-18; Owner=S-1-5-32-544; DiscretionaryAcl(AceType=AccessDenied; AceFlags=None; AuditFlags=None; InheritnaceFlags=None; IsInherited=False; PropagationFlags=None; User=S-1-5-7); DiscretionaryAcl(AceType=AccessDenied; AceFlags=None; AuditFlags=None; InheritnaceFlags=None; IsInherited=False; PropagationFlags=None; User=S-1-5-32-546); DiscretionaryAcl(AceType=AccessAllowed; AceFlags=None; AuditFlags=None; InheritnaceFlags=None; IsInherited=False; PropagationFlags=None; User=S-1-5-18); DiscretionaryAcl(AceType=AccessAllowed; AceFlags=None; AuditFlags=None; InheritnaceFlags=None; IsInherited=False; PropagationFlags=None; User=S-1-5-32-544)");
        }

        /// <summary>
        /// Method Name : DecodeSsdl
        /// 
        /// Method Desciption : Decoded ssdl codes
        /// 
        /// Test Scenerio : If domainBuffer parameter is not null
        /// 
        /// Known Input :
        ///     * ssdlStr = O:BAG:SYD:(D;;0xf0007;;;AN)(D;;0xf0007;;;BG)(A;;0xf0005;;;SY)(A;;0x5;;;BA)
        ///     * decodeBuffer = new StringBuilder()
        ///     * domainBuffer = null
        ///     * usernameBuffer = null
        ///     * formatted = 0
        /// 
        /// Expected Output : 
        ///     * ControlFlags=DiscretionaryAclPresent, SelfRelative; ResourceManagerControlBits=0; Group=S-1-5-18; Owner=S-1-5-32-544; DiscretionaryAcl(AceType=AccessDenied; AceFlags=None; AuditFlags=None; InheritnaceFlags=None; IsInherited=False; PropagationFlags=None; User=S-1-5-7); DiscretionaryAcl(AceType=AccessDenied; AceFlags=None; AuditFlags=None; InheritnaceFlags=None; IsInherited=False; PropagationFlags=None; User=S-1-5-32-546); DiscretionaryAcl(AceType=AccessAllowed; AceFlags=None; AuditFlags=None; InheritnaceFlags=None; IsInherited=False; PropagationFlags=None; User=S-1-5-18); DiscretionaryAcl(AceType=AccessAllowed; AceFlags=None; AuditFlags=None; InheritnaceFlags=None; IsInherited=False; PropagationFlags=None; User=S-1-5-32-544)
        /// </summary>
        public void DecodeSsdl_IfDomainBufferIsNotNull_ReturnString()
        {
            //Act
            const string ssdlStr = "O:BAG:SYD:(D;;0xf0007;;;AN)(D;;0xf0007;;;BG)(A;;0xf0005;;;SY)(A;;0x5;;;BA)";
            var decodeBuffer = new StringBuilder();
            var domainBuffer = new StringBuilder();
            StringBuilder usernameBuffer = null;
            var formatted = 0;

            //Arrange
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = SsdlHelper.DecodeSsdl(ssdlStr, decodeBuffer, domainBuffer, usernameBuffer, ref formatted);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "ControlFlags=DiscretionaryAclPresent, SelfRelative; ResourceManagerControlBits=0; Group=S-1-5-18; Owner=S-1-5-32-544; DiscretionaryAcl(AceType=AccessDenied; AceFlags=None; AuditFlags=None; InheritnaceFlags=None; IsInherited=False; PropagationFlags=None; User=S-1-5-7); DiscretionaryAcl(AceType=AccessDenied; AceFlags=None; AuditFlags=None; InheritnaceFlags=None; IsInherited=False; PropagationFlags=None; User=S-1-5-32-546); DiscretionaryAcl(AceType=AccessAllowed; AceFlags=None; AuditFlags=None; InheritnaceFlags=None; IsInherited=False; PropagationFlags=None; User=S-1-5-18); DiscretionaryAcl(AceType=AccessAllowed; AceFlags=None; AuditFlags=None; InheritnaceFlags=None; IsInherited=False; PropagationFlags=None; User=S-1-5-32-544)");
        }
    }
}
