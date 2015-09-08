// File provided for Reference Use Only by Microsoft Corporation (c) 2007.
// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class: CoTaskMemUnicodeSafeHandle
**
** Purpose:
** This internal class is a SafeHandle implementation over a
** native CoTaskMem allocated via SecureStringToCoTaskMemUnicode.
**
============================================================*/
using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
 
namespace System.Diagnostics.Eventing.Reader {
 
    //
    // Marked as SecurityCritical due to link demands from inherited
    // SafeHandle members.
    //
#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    public sealed class CoTaskMemUnicodeSafeHandle : SafeHandle {
 
        public CoTaskMemUnicodeSafeHandle()
            : base(IntPtr.Zero, true) {
        }
 
        public CoTaskMemUnicodeSafeHandle(IntPtr handle, bool ownsHandle)
            : base(IntPtr.Zero, ownsHandle) {
            SetHandle(handle);
        }
 
        public void SetMemory(IntPtr handle) {
            SetHandle(handle);
        }
 
        public IntPtr GetMemory() {
            return handle;
        }
  
 
        public override bool IsInvalid {
            get {
                return IsClosed || handle == IntPtr.Zero;
            }
        }
  
        protected override bool ReleaseHandle() {
            Marshal.ZeroFreeCoTaskMemUnicode(handle);
            handle = IntPtr.Zero;
            return true;
        }
  
        // DONT compare CoTaskMemUnicodeSafeHandle with CoTaskMemUnicodeSafeHandle.Zero
        // use IsInvalid instead. Zero is provided where a NULL handle needed
        public static CoTaskMemUnicodeSafeHandle Zero {
            get {
                return new CoTaskMemUnicodeSafeHandle();
            }
        }
    }
}
 
// File provided for Reference Use Only by Microsoft Corporation (c) 2007.