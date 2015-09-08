/*
 * Event Log Recorder
 * Copyright (C) 2008 Erdoðan Kalemci <olligan@gmail.com>
 * You have no rights to distrubute, modify and use this code unless writer gives permission
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Parser;
using Log;
using Microsoft.Win32;
using System.Globalization;

namespace Parser
{
    public partial class EventLogRecorder : AppParser
    {
        //
        //  Primary language IDs.
        //

        private const uint LANG_NEUTRAL                     = 0x00;
        private const uint LANG_INVARIANT                   = 0x7f;

        private const uint LANG_AFRIKAANS                   = 0x36;
        private const uint LANG_ALBANIAN                    = 0x1c;
        private const uint LANG_ARABIC                      = 0x01;
        private const uint LANG_ARMENIAN                    = 0x2b;
        private const uint LANG_ASSAMESE                    = 0x4d;
        private const uint LANG_AZERI                       = 0x2c;
        private const uint LANG_BASQUE                      = 0x2d;
        private const uint LANG_BELARUSIAN                  = 0x23;
        private const uint LANG_BENGALI                     = 0x45;
        private const uint LANG_BULGARIAN                   = 0x02;
        private const uint LANG_CATALAN                     = 0x03;
        private const uint LANG_CHINESE                     = 0x04;
        private const uint LANG_CROATIAN                    = 0x1a;
        private const uint LANG_CZECH                       = 0x05;
        private const uint LANG_DANISH                      = 0x06;
        private const uint LANG_DUTCH                       = 0x13;
        private const uint LANG_ENGLISH                     = 0x09;
        private const uint LANG_ESTONIAN                    = 0x25;
        private const uint LANG_FAEROESE                    = 0x38;
        private const uint LANG_FARSI                       = 0x29;
        private const uint LANG_FINNISH                     = 0x0b;
        private const uint LANG_FRENCH                      = 0x0c;
        private const uint LANG_GEORGIAN                    = 0x37;
        private const uint LANG_GERMAN                      = 0x07;
        private const uint LANG_GREEK                       = 0x08;
        private const uint LANG_GUJARATI                    = 0x47;
        private const uint LANG_HEBREW                      = 0x0d;
        private const uint LANG_HINDI                       = 0x39;
        private const uint LANG_HUNGARIAN                   = 0x0e;
        private const uint LANG_ICELANDIC                   = 0x0f;
        private const uint LANG_INDONESIAN                  = 0x21;
        private const uint LANG_ITALIAN                     = 0x10;
        private const uint LANG_JAPANESE                    = 0x11;
        private const uint LANG_KANNADA                     = 0x4b;
        private const uint LANG_KASHMIRI                    = 0x60;
        private const uint LANG_KAZAK                       = 0x3f;
        private const uint LANG_KONKANI                     = 0x57;
        private const uint LANG_KOREAN                      = 0x12;
        private const uint LANG_LATVIAN                     = 0x26;
        private const uint LANG_LITHUANIAN                  = 0x27;
        private const uint LANG_MACEDONIAN                  = 0x2f;
        private const uint LANG_MALAY                       = 0x3e;
        private const uint LANG_MALAYALAM                   = 0x4c;
        private const uint LANG_MANIPURI                    = 0x58;
        private const uint LANG_MARATHI                     = 0x4e;
        private const uint LANG_NEPALI                      = 0x61;
        private const uint LANG_NORWEGIAN                   = 0x14;
        private const uint LANG_ORIYA                       = 0x48;
        private const uint LANG_POLISH                      = 0x15;
        private const uint LANG_PORTUGUESE                  = 0x16;
        private const uint LANG_PUNJABI                     = 0x46;
        private const uint LANG_ROMANIAN                    = 0x18;
        private const uint LANG_RUSSIAN                     = 0x19;
        private const uint LANG_SANSKRIT                    = 0x4f;
        private const uint LANG_SERBIAN                     = 0x1a;
        private const uint LANG_SINDHI                      = 0x59;
        private const uint LANG_SLOVAK                      = 0x1b;
        private const uint LANG_SLOVENIAN                   = 0x24;
        private const uint LANG_SPANISH                     = 0x0a;
        private const uint LANG_SWAHILI                     = 0x41;
        private const uint LANG_SWEDISH                     = 0x1d;
        private const uint LANG_TAMIL                       = 0x49;
        private const uint LANG_TATAR                       = 0x44;
        private const uint LANG_TELUGU                      = 0x4a;
        private const uint LANG_THAI                        = 0x1e;
        private const uint LANG_TURKISH                     = 0x1f;
        private const uint LANG_UKRAINIAN                   = 0x22;
        private const uint LANG_URDU                        = 0x20;
        private const uint LANG_UZBEK                       = 0x43;
        private const uint LANG_VIETNAMESE                  = 0x2a;

        //
        //  Sublanguage IDs.
        //
        //  The name immediately following SUBLANG_ dictates which primary
        //  language ID that sublanguage ID can be combined with to form a
        //  valid language ID.
        //

        private const uint SUBLANG_DEFAULT = 0x01;    // user default



        [DllImport("advapi32.dll", SetLastError = true, EntryPoint = "OpenEventLog")]
        public static extern IntPtr OpenEventLog(
            [MarshalAs(UnmanagedType.LPStr)] String lpUNCServerName,
            [MarshalAs(UnmanagedType.LPStr)] String lpSourceName);

        [DllImport("advapi32.dll", SetLastError = true, EntryPoint = "CloseEventLog")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseEventLog(IntPtr hEventLog);

        [DllImport("advapi32.dll", SetLastError = true, EntryPoint = "ReadEventLog")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReadEventLog(
            IntPtr hEventLog,
            Int32 dwReadFlags,
            UInt32 dwRecordOffset,
            [Out()] byte[] lpBuffer,
          Int32 nNumberOfBytesToRead,
          ref Int32 pnBytesRead,
          ref Int32 pnMinNumberOfBytesNeeded);

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll")]
        static extern uint FormatMessage(uint dwFlags, IntPtr lpSource,
           uint dwMessageId, uint dwLanguageId, [Out] StringBuilder lpBuffer,
           uint nSize, IntPtr Arguments);

        [DllImport("kernel32.dll")]
        static extern uint FormatMessage(uint dwFlags, IntPtr lpSource,
           uint dwMessageId, uint dwLanguageId, [Out] StringBuilder lpBuffer,
           uint nSize, String[] Arguments);

        // the version, the sample is built upon:
        [DllImport("Kernel32.dll", SetLastError=true)]
        static extern uint FormatMessage( uint dwFlags, IntPtr lpSource,
           uint dwMessageId, uint dwLanguageId, ref IntPtr lpBuffer,
           uint nSize, IntPtr pArguments);

        // the parameters can also be passed as a string array:
        [DllImport("Kernel32.dll", SetLastError=true)]
        static extern uint FormatMessage( uint dwFlags, IntPtr lpSource,
           uint dwMessageId, uint dwLanguageId, ref IntPtr lpBuffer,
           uint nSize, string[] Arguments);

        public static UInt32 MAKELANGID(UInt32 primary, UInt32 sub)
        {
            return (UInt32)(((UInt16)sub) << 10) | ((UInt16)primary);
        }

        public static Int32 PRIMARYLANGID(Int32 lcid)
        {
            return ((UInt16)lcid) & 0x3ff;
        }

        public static Int32 SUBLANGID(Int32 lcid)
        {
            return ((UInt16)lcid) >> 10;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct EVENTLOGRECORD
        {
            public Int32 Length;
            public Int32 Reserved;
            public Int32 RecordNumber;
            public Int32 TimeGenerated;
            public Int32 TimeWritten;
            public Int32 EventID;
            public Int16 EventType;
            public Int16 NumStrings;
            public Int16 EventCategory;
            public Int16 ReservedFlags;
            public Int32 ClosingRecordNumber;
            public Int32 StringOffset;
            public Int32 UserSidLength;
            public Int32 UserSidOffset;
            public Int32 DataLength;
            public Int32 DataOffset;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        struct SID_IDENTIFIER_AUTHORITY 
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=6)]
            Byte [] Value;
        };

        public enum ReadFlags
        {
            EVENTLOG_SEQUENTIAL_READ = 0x0001,
            EVENTLOG_SEEK_READ = 0x0002,
            EVENTLOG_FORWARDS_READ = 0x0004,
            EVENTLOG_BACKWARDS_READ = 0x0008
        };

        public enum EventType
        {
            Error = 0x0001,
            Warning = 0x0002,
            Information = 0x0004,
            Success = 0x0008,
            Failure = 0x0010
        };

        enum SID_NAME_USE
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

        private const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
        private const uint DONT_RESOLVE_DLL_REFERENCES = 0x00000001;
        private const uint LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008;
        private const uint LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010;
        private const uint LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040;

        private const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        private const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        private const uint FORMAT_MESSAGE_FROM_STRING = 0x00000400;
        private const uint FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;
        private const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        private const uint FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
    }
}
