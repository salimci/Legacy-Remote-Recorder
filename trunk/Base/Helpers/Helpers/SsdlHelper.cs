using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace Natek.Helpers.Security.AccessControl
{
    public static class SsdlHelper
    {
        public static string DecodeSsdl(string ssdlStr, StringBuilder decodeBuffer,
            StringBuilder domainBuffer, StringBuilder usernameBuffer, ref int formatted)
        {
            try
            {
                var ssdl = new RawSecurityDescriptor(ssdlStr);
                ++formatted;
                decodeBuffer.Remove(0, decodeBuffer.Length);
                decodeBuffer.Append("ControlFlags=")
                  .Append(ssdl.ControlFlags)
                  .Append("; ResourceManagerControlBits=")
                  .Append(ssdl.ResourceManagerControl);
                if (ssdl.Group != null)
                    decodeBuffer.Append("; Group=").Append(ssdl.Group);
                if (ssdl.Owner != null)
                    decodeBuffer.Append("; Owner=").Append(ssdl.Owner);
                if (ssdl.DiscretionaryAcl != null)
                {
                    foreach (var acl in ssdl.DiscretionaryAcl)
                    {
                        decodeBuffer.Append("; DiscretionaryAcl");
                        DecodeAcl(acl, decodeBuffer, domainBuffer, usernameBuffer);
                    }
                }
                if (ssdl.SystemAcl != null)
                {
                    foreach (var acl in ssdl.SystemAcl)
                    {
                        decodeBuffer.Append("; SystemAcl");
                        DecodeAcl(acl, decodeBuffer, domainBuffer, usernameBuffer);
                    }
                }
                return decodeBuffer.ToString();
            }
            catch
            {
                return ssdlStr;
            }
        }

        public static void DecodeAcl(GenericAce acl, StringBuilder sb, StringBuilder domainSb, StringBuilder usernameSb)
        {
            sb.Append("(")
                      .Append("AceType=")
                      .Append(acl.AceType)
                      .Append("; AceFlags=")
                      .Append(acl.AceFlags)
                      .Append("; AuditFlags=")
                      .Append(acl.AuditFlags)
                      .Append("; InheritnaceFlags=")
                      .Append(acl.InheritanceFlags)
                      .Append("; IsInherited=")
                      .Append(acl.IsInherited)
                      .Append("; PropagationFlags=")
                      .Append(acl.PropagationFlags);
            try
            {
                var k = acl as KnownAce;
                if (k != null)
                    GetUser(k.SecurityIdentifier, sb, domainSb, usernameSb);
            }
            catch
            {
            }
            sb.Append(")");
        }

        public static void GetUser(SecurityIdentifier sid, StringBuilder sb, StringBuilder referencedDomainName, StringBuilder name)
        {
            try
            {
                if (sid == null)
                    return;
                sb.Append("; User=");
                referencedDomainName.Remove(0, referencedDomainName.Length);
                name.Remove(0, name.Length);

                var b = new byte[sid.BinaryLength];
                sid.GetBinaryForm(b, 0);
                var cchName = (uint)name.Capacity;
                var cchReferencedDomainName = (uint)referencedDomainName.Capacity;
                SidHelper.SidNameUse sidUse;
                if (SidHelper.LookupAccountSid(null, b, name, ref cchName, referencedDomainName,
                                               ref cchReferencedDomainName,
                                               out sidUse))
                {
                    if (referencedDomainName.Length > 0)
                        sb.Append(referencedDomainName).Append('\\');
                    sb.Append(name);
                    return;
                }
            }
            catch
            {
            }
            sb.Append(sid);
        }
    }
}
