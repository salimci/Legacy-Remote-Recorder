using System.Runtime.InteropServices;
using System.Text;

namespace Natek.Helpers.Security.AccessControl
{
    public static class SidHelper
    {
        public static readonly int NoError = 0;
        public static readonly int ErrorInsufficientBuffer = 122;

        public enum SidNameUse
        {
            SidTypeUser = 1,
            SidTypeGroup,
            SidTypeDomain,
            SidTypeAlias,
            SidTypeWellKnownGroup,
            SidTypeDeletedAccount,
            SidTypeInvalid,
            SidTypeUnknown,
            SidTypeComputer
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool LookupAccountSid(
          string lpSystemName,
          [MarshalAs(UnmanagedType.LPArray)] byte[] sid,
          StringBuilder lpName,
          ref uint cchName,
          StringBuilder referencedDomainName,
          ref uint cchReferencedDomainName,
          out SidNameUse peUse);
    }
}
