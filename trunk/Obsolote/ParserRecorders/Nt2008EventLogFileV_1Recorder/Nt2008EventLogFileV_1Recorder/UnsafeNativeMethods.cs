using System;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

public class UnsafeNativeMethods
{
    public static readonly int ERROR_INSUFFICIENT_BUFFER = 122;

    public static readonly int ERROR_EVT_MESSAGE_NOT_FOUND = 15027;

    public static readonly int ERROR_EVT_MESSAGE_ID_NOT_FOUND = 15028;

    public static readonly int ERROR_SUCCESS = 0;

    public static readonly int ERROR_UNHANDLED_EXCEPTION = 574;
    //
    // EventLog
    //
    [Flags]
    public enum EvtQueryFlags
    {
        EvtQueryChannelPath = 0x1,
        EvtQueryFilePath = 0x2,
        EvtQueryForwardDirection = 0x100,
        EvtQueryReverseDirection = 0x200,
        EvtQueryTolerateQueryErrors = 0x1000
    }

    [Flags]
    public enum EvtSubscribeFlags
    {
        EvtSubscribeToFutureEvents = 1,
        EvtSubscribeStartAtOldestRecord = 2,
        EvtSubscribeStartAfterBookmark = 3,
        EvtSubscribeTolerateQueryErrors = 0x1000,
        EvtSubscribeStrict = 0x10000
    }

    public enum NativeErrorCodes : uint
    {
        ERROR_SUCCESS = 0,
        ERROR_INVALID_PARAMETER = 87,
        ERROR_INSUFFICIENT_BUFFER = 122,
        ERROR_NO_MORE_ITEMS = 259,
        ERROR_RESOURCE_LANG_NOT_FOUND = 1815,
        ERROR_EVT_MESSAGE_NOT_FOUND = 15027,
        ERROR_EVT_MESSAGE_ID_NOT_FOUND = 15028,
        ERROR_EVT_UNRESOLVED_VALUE_INSERT = 15029,
        ERROR_EVT_MESSAGE_LOCALE_NOT_FOUND = 15033,
        ERROR_MUI_FILE_NOT_FOUND = 15100
    }


    /// <summary>
    /// Evt Variant types
    /// </summary>
    public enum EvtVariantType
    {
        EvtVarTypeNull = 0,
        EvtVarTypeString = 1,
        EvtVarTypeAnsiString = 2,
        EvtVarTypeSByte = 3,
        EvtVarTypeByte = 4,
        EvtVarTypeInt16 = 5,
        EvtVarTypeUInt16 = 6,
        EvtVarTypeInt32 = 7,
        EvtVarTypeUInt32 = 8,
        EvtVarTypeInt64 = 9,
        EvtVarTypeUInt64 = 10,
        EvtVarTypeSingle = 11,
        EvtVarTypeDouble = 12,
        EvtVarTypeBoolean = 13,
        EvtVarTypeBinary = 14,
        EvtVarTypeGuid = 15,
        EvtVarTypeSizeT = 16,
        EvtVarTypeFileTime = 17,
        EvtVarTypeSysTime = 18,
        EvtVarTypeSid = 19,
        EvtVarTypeHexInt32 = 20,
        EvtVarTypeHexInt64 = 21,
        // these types used internally
        EvtVarTypeEvtHandle = 32,
        EvtVarTypeEvtXml = 35,
        //Array = 128
        EvtVarTypeStringArray = 129,
        EvtVarTypeUInt32Array = 136
    }

    public enum EvtMasks
    {
        EVT_VARIANT_TYPE_MASK = 0x7f,
        EVT_VARIANT_TYPE_ARRAY = 128
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Auto)]
    public struct EvtVariant
    {
        [FieldOffset(0)]
        public UInt32 UInteger;
        [FieldOffset(0)]
        public Int32 Integer;
        [FieldOffset(0)]
        public byte UInt8;
        [FieldOffset(0)]
        public short Short;
        [FieldOffset(0)]
        public ushort UShort;
        [FieldOffset(0)]
        public UInt32 Bool;
        [FieldOffset(0)]
        public Byte ByteVal;
        [FieldOffset(0)]
        public byte SByte;
        [FieldOffset(0)]
        public UInt64 ULong;
        [FieldOffset(0)]
        public Int64 Long;
        [FieldOffset(0)]
        public Double Double;
        [FieldOffset(0)]
        public IntPtr StringVal;
        [FieldOffset(0)]
        public IntPtr AnsiString;
        [FieldOffset(0)]
        public IntPtr SidVal;
        [FieldOffset(0)]
        public IntPtr Binary;
        [FieldOffset(0)]
        public IntPtr Reference;
        [FieldOffset(0)]
        public IntPtr Handle;
        [FieldOffset(0)]
        public IntPtr GuidReference;
        [FieldOffset(0)]
        public UInt64 FileTime;
        [FieldOffset(0)]
        public IntPtr SystemTime;
        [FieldOffset(8)]
        public UInt32 Count;   // number of elements (not length) in bytes.
        [FieldOffset(12)]
        public UInt32 Type;
    }

    public enum EvtEventPropertyId
    {
        EvtEventQueryIDs = 0,
        EvtEventPath = 1
    }

    /// <summary>
    /// The query flags to get information about query
    /// </summary>
    public enum EvtQueryPropertyId
    {
        EvtQueryNames = 0,   //String;   //Variant will be array of EvtVarTypeString
        EvtQueryStatuses = 1 //UInt32;   //Variant will be Array of EvtVarTypeUInt32
    }

    /// <summary>
    /// Publisher Metadata properties
    /// </summary>
    public enum EvtPublisherMetadataPropertyId
    {
        EvtPublisherMetadataPublisherGuid = 0,      // EvtVarTypeGuid
        EvtPublisherMetadataResourceFilePath = 1,       // EvtVarTypeString
        EvtPublisherMetadataParameterFilePath = 2,      // EvtVarTypeString
        EvtPublisherMetadataMessageFilePath = 3,        // EvtVarTypeString
        EvtPublisherMetadataHelpLink = 4,               // EvtVarTypeString
        EvtPublisherMetadataPublisherMessageID = 5,     // EvtVarTypeUInt32

        EvtPublisherMetadataChannelReferences = 6,      // EvtVarTypeEvtHandle, ObjectArray
        EvtPublisherMetadataChannelReferencePath = 7,   // EvtVarTypeString
        EvtPublisherMetadataChannelReferenceIndex = 8,  // EvtVarTypeUInt32
        EvtPublisherMetadataChannelReferenceID = 9,     // EvtVarTypeUInt32
        EvtPublisherMetadataChannelReferenceFlags = 10,  // EvtVarTypeUInt32
        EvtPublisherMetadataChannelReferenceMessageID = 11, // EvtVarTypeUInt32

        EvtPublisherMetadataLevels = 12,                 // EvtVarTypeEvtHandle, ObjectArray
        EvtPublisherMetadataLevelName = 13,              // EvtVarTypeString
        EvtPublisherMetadataLevelValue = 14,             // EvtVarTypeUInt32
        EvtPublisherMetadataLevelMessageID = 15,         // EvtVarTypeUInt32

        EvtPublisherMetadataTasks = 16,                  // EvtVarTypeEvtHandle, ObjectArray
        EvtPublisherMetadataTaskName = 17,               // EvtVarTypeString
        EvtPublisherMetadataTaskEventGuid = 18,          // EvtVarTypeGuid
        EvtPublisherMetadataTaskValue = 19,              // EvtVarTypeUInt32
        EvtPublisherMetadataTaskMessageID = 20,          // EvtVarTypeUInt32

        EvtPublisherMetadataOpcodes = 21,                // EvtVarTypeEvtHandle, ObjectArray
        EvtPublisherMetadataOpcodeName = 22,             // EvtVarTypeString
        EvtPublisherMetadataOpcodeValue = 23,            // EvtVarTypeUInt32
        EvtPublisherMetadataOpcodeMessageID = 24,        // EvtVarTypeUInt32

        EvtPublisherMetadataKeywords = 25,               // EvtVarTypeEvtHandle, ObjectArray
        EvtPublisherMetadataKeywordName = 26,            // EvtVarTypeString
        EvtPublisherMetadataKeywordValue = 27,           // EvtVarTypeUInt64
        EvtPublisherMetadataKeywordMessageID = 28//,       // EvtVarTypeUInt32
        //EvtPublisherMetadataPropertyIdEND
    }

    public enum EvtChannelReferenceFlags
    {
        EvtChannelReferenceImported = 1
    }

    public enum EvtEventMetadataPropertyId
    {
        EventMetadataEventID,       // EvtVarTypeUInt32
        EventMetadataEventVersion,  // EvtVarTypeUInt32
        EventMetadataEventChannel,  // EvtVarTypeUInt32
        EventMetadataEventLevel,    // EvtVarTypeUInt32
        EventMetadataEventOpcode,   // EvtVarTypeUInt32
        EventMetadataEventTask,     // EvtVarTypeUInt32
        EventMetadataEventKeyword,  // EvtVarTypeUInt64
        EventMetadataEventMessageID,// EvtVarTypeUInt32
        EventMetadataEventTemplate // EvtVarTypeString
        //EvtEventMetadataPropertyIdEND
    }

    //CHANNEL CONFIGURATION
    public enum EvtChannelConfigPropertyId
    {
        EvtChannelConfigEnabled = 0,            // EvtVarTypeBoolean
        EvtChannelConfigIsolation,              // EvtVarTypeUInt32, EVT_CHANNEL_ISOLATION_TYPE
        EvtChannelConfigType,                   // EvtVarTypeUInt32, EVT_CHANNEL_TYPE
        EvtChannelConfigOwningPublisher,        // EvtVarTypeString
        EvtChannelConfigClassicEventlog,        // EvtVarTypeBoolean
        EvtChannelConfigAccess,                 // EvtVarTypeString
        EvtChannelLoggingConfigRetention,       // EvtVarTypeBoolean
        EvtChannelLoggingConfigAutoBackup,      // EvtVarTypeBoolean
        EvtChannelLoggingConfigMaxSize,         // EvtVarTypeUInt64
        EvtChannelLoggingConfigLogFilePath,     // EvtVarTypeString
        EvtChannelPublishingConfigLevel,        // EvtVarTypeUInt32
        EvtChannelPublishingConfigKeywords,     // EvtVarTypeUInt64
        EvtChannelPublishingConfigControlGuid,  // EvtVarTypeGuid
        EvtChannelPublishingConfigBufferSize,   // EvtVarTypeUInt32
        EvtChannelPublishingConfigMinBuffers,   // EvtVarTypeUInt32
        EvtChannelPublishingConfigMaxBuffers,   // EvtVarTypeUInt32
        EvtChannelPublishingConfigLatency,      // EvtVarTypeUInt32
        EvtChannelPublishingConfigClockType,    // EvtVarTypeUInt32, EVT_CHANNEL_CLOCK_TYPE
        EvtChannelPublishingConfigSidType,      // EvtVarTypeUInt32, EVT_CHANNEL_SID_TYPE
        EvtChannelPublisherList,                // EvtVarTypeString | EVT_VARIANT_TYPE_ARRAY
        EvtChannelConfigPropertyIdEND
    }

    //LOG INFORMATION
    public enum EvtLogPropertyId
    {
        EvtLogCreationTime = 0,             // EvtVarTypeFileTime
        EvtLogLastAccessTime,               // EvtVarTypeFileTime
        EvtLogLastWriteTime,                // EvtVarTypeFileTime
        EvtLogFileSize,                     // EvtVarTypeUInt64
        EvtLogAttributes,                   // EvtVarTypeUInt32
        EvtLogNumberOfLogRecords,           // EvtVarTypeUInt64
        EvtLogOldestRecordNumber,           // EvtVarTypeUInt64
        EvtLogFull,                         // EvtVarTypeBoolean
    }

    public enum EvtExportLogFlags
    {
        EvtExportLogChannelPath = 1,
        EvtExportLogFilePath = 2,
        EvtExportLogTolerateQueryErrors = 0x1000
    }

    //RENDERING
    public enum EvtRenderContextFlags
    {
        EvtRenderContextValues = 0,      // Render specific properties
        EvtRenderContextSystem = 1,      // Render all system properties (System)
        EvtRenderContextUser = 2         // Render all user properties (User/EventData)
    }

    public enum EvtRenderFlags
    {
        EvtRenderEventValues = 0,       // Variants
        EvtRenderEventXml = 1,          // XML
        EvtRenderBookmark = 2           // Bookmark
    }

    public enum EvtFormatMessageFlags
    {
        EvtFormatMessageEvent = 1,
        EvtFormatMessageLevel = 2,
        EvtFormatMessageTask = 3,
        EvtFormatMessageOpcode = 4,
        EvtFormatMessageKeyword = 5,
        EvtFormatMessageChannel = 6,
        EvtFormatMessageProvider = 7,
        EvtFormatMessageId = 8,
        EvtFormatMessageXml = 9
    }

    public enum EvtSystemPropertyId
    {
        EvtSystemProviderName = 0,          // EvtVarTypeString
        EvtSystemProviderGuid,              // EvtVarTypeGuid
        EvtSystemEventID,                   // EvtVarTypeUInt16
        EvtSystemQualifiers,                // EvtVarTypeUInt16
        EvtSystemLevel,                     // EvtVarTypeUInt8
        EvtSystemTask,                      // EvtVarTypeUInt16
        EvtSystemOpcode,                    // EvtVarTypeUInt8
        EvtSystemKeywords,                  // EvtVarTypeHexInt64
        EvtSystemTimeCreated,               // EvtVarTypeFileTime
        EvtSystemEventRecordId,             // EvtVarTypeUInt64
        EvtSystemActivityID,                // EvtVarTypeGuid
        EvtSystemRelatedActivityID,         // EvtVarTypeGuid
        EvtSystemProcessID,                 // EvtVarTypeUInt32
        EvtSystemThreadID,                  // EvtVarTypeUInt32
        EvtSystemChannel,                   // EvtVarTypeString
        EvtSystemComputer,                  // EvtVarTypeString
        EvtSystemUserID,                    // EvtVarTypeSid
        EvtSystemVersion,                   // EvtVarTypeUInt8
        EvtSystemPropertyIdEND
    }

    //SESSION
    public enum EvtLoginClass
    {
        EvtRpcLogin = 1
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct EvtRpcLogin
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Server;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string User;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Domain;
        public CoTaskMemUnicodeSafeHandle Password;
        public int Flags;
    }


    //SEEK
    [Flags]
    public enum EvtSeekFlags
    {
        EvtSeekRelativeToFirst = 1,
        EvtSeekRelativeToLast = 2,
        EvtSeekRelativeToCurrent = 3,
        EvtSeekRelativeToBookmark = 4,
        EvtSeekOriginMask = 7,
        EvtSeekStrict = 0x10000
    }

    [DllImport("wevtapi.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern IntPtr EvtQuery(
                        IntPtr session,
                        [MarshalAs(UnmanagedType.LPWStr)]string path,
                        [MarshalAs(UnmanagedType.LPWStr)]string query,
                        int flags);

    //SEEK
    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtSeek(
                        IntPtr resultSet,
                        long position,
                        IntPtr bookmark,
                        int timeout,
                        [MarshalAs(UnmanagedType.I4)]EvtSeekFlags flags
                                    );

    [DllImport("wevtapi.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern IntPtr EvtSubscribe(
                        IntPtr session,
                        SafeWaitHandle signalEvent,
                        [MarshalAs(UnmanagedType.LPWStr)]string path,
                        [MarshalAs(UnmanagedType.LPWStr)]string query,
                        IntPtr bookmark,
                        IntPtr context,
                        IntPtr callback,
                        int flags);

    [DllImport("wevtapi.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtNext(
                        IntPtr queryHandle,
                        int eventSize,
                        [MarshalAs(UnmanagedType.LPArray)] IntPtr[] events,
                        int timeout,
                        int flags,
                        ref int returned);

    [DllImport("wevtapi.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtCancel(IntPtr handle);

    [DllImport("wevtapi.dll")]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtClose(IntPtr handle);

    /*
    [DllImport("wevtapi.dll", EntryPoint = "EvtClose", SetLastError = true)]
    public static extern bool EvtClose(
                        IntPtr eventHandle
                                       );
     */

    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtGetEventInfo(
                        IntPtr eventHandle,
        //int propertyId
                        [MarshalAs(UnmanagedType.I4)]EvtEventPropertyId propertyId,
                        int bufferSize,
                        IntPtr bufferPtr,
                        out int bufferUsed
                                        );

    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtGetQueryInfo(
                        IntPtr queryHandle,
                        [MarshalAs(UnmanagedType.I4)]EvtQueryPropertyId propertyId,
                        int bufferSize,
                        IntPtr buffer,
                        ref int bufferRequired
                                        );

    //PUBLISHER METADATA
    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr EvtOpenPublisherMetadata(
                        IntPtr session,
                        [MarshalAs(UnmanagedType.LPWStr)] string publisherId,
                        [MarshalAs(UnmanagedType.LPWStr)] string logFilePath,
                        int locale,
                        int flags
                                );

    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtGetPublisherMetadataProperty(
                        IntPtr publisherMetadataHandle,
                        [MarshalAs(UnmanagedType.I4)] EvtPublisherMetadataPropertyId propertyId,
                        int flags,
                        int publisherMetadataPropertyBufferSize,
                        IntPtr publisherMetadataPropertyBuffer,
                        out int publisherMetadataPropertyBufferUsed
                                );

    //NEW

    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtGetObjectArraySize(
                        IntPtr objectArray,
                        out int objectArraySize
                                    );

    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtGetObjectArrayProperty(
                        IntPtr objectArray,
                        int propertyId,
                        int arrayIndex,
                        int flags,
                        int propertyValueBufferSize,
                        IntPtr propertyValueBuffer,
                        out int propertyValueBufferUsed
                                        );

    //NEW 2
    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr EvtOpenEventMetadataEnum(
                        IntPtr publisherMetadata,
                        int flags
                                );

    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //public static extern IntPtr EvtNextEventMetadata(
    public static extern IntPtr EvtNextEventMetadata(
                        IntPtr eventMetadataEnum,
                        int flags
                                );

    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtGetEventMetadataProperty(
                        IntPtr eventMetadata,
                        [MarshalAs(UnmanagedType.I4)]  EvtEventMetadataPropertyId propertyId,
                        int flags,
                        int eventMetadataPropertyBufferSize,
                        IntPtr eventMetadataPropertyBuffer,
                        out int eventMetadataPropertyBufferUsed
                               );


    //Channel Configuration Native Api

    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr EvtOpenChannelEnum(
                        IntPtr session,
                        int flags
                                );

    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtNextChannelPath(
                        IntPtr channelEnum,
                        int channelPathBufferSize,
        //StringBuilder channelPathBuffer,
                        [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder channelPathBuffer,
                        out int channelPathBufferUsed
                                );


    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr EvtOpenPublisherEnum(
                        IntPtr session,
                        int flags
                                );

    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtNextPublisherId(
                        IntPtr publisherEnum,
                        int publisherIdBufferSize,
                        [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder publisherIdBuffer,
                        out int publisherIdBufferUsed
                                );

    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr EvtOpenChannelConfig(
                        IntPtr session,
                        [MarshalAs(UnmanagedType.LPWStr)]String channelPath,
                        int flags
                                );

    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtSaveChannelConfig(
                        IntPtr channelConfig,
                        int flags
                                );


    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtSetChannelConfigProperty(
                        IntPtr channelConfig,
                        [MarshalAs(UnmanagedType.I4)]EvtChannelConfigPropertyId propertyId,
                        int flags,
                        ref EvtVariant propertyValue
                                );


    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtGetChannelConfigProperty(
                        IntPtr channelConfig,
                        [MarshalAs(UnmanagedType.I4)]EvtChannelConfigPropertyId propertyId,
                        int flags,
                        int propertyValueBufferSize,
                        IntPtr propertyValueBuffer,
                        out int propertyValueBufferUsed
                               );

    //Log Information Native Api

    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr EvtOpenLog(
                        IntPtr session,
                        [MarshalAs(UnmanagedType.LPWStr)] string path,
                        [MarshalAs(UnmanagedType.I4)]PathType flags
                                );


    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtGetLogInfo(
                        IntPtr log,
                        [MarshalAs(UnmanagedType.I4)]EvtLogPropertyId propertyId,
                        int propertyValueBufferSize,
                        IntPtr propertyValueBuffer,
                        out int propertyValueBufferUsed
                                );

    //LOG MANIPULATION

    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtExportLog(
                        IntPtr session,
                        [MarshalAs(UnmanagedType.LPWStr)]string channelPath,
                        [MarshalAs(UnmanagedType.LPWStr)]string query,
                        [MarshalAs(UnmanagedType.LPWStr)]string targetFilePath,
                        int flags
                                    );

    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtArchiveExportedLog(
                        IntPtr session,
                        [MarshalAs(UnmanagedType.LPWStr)]string logFilePath,
                        int locale,
                        int flags
                                    );

    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtClearLog(
                        IntPtr session,
                        [MarshalAs(UnmanagedType.LPWStr)]string channelPath,
                        [MarshalAs(UnmanagedType.LPWStr)]string targetFilePath,
                        int flags
                                    );

    //RENDERING
    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr EvtCreateRenderContext(
                        Int32 valuePathsCount,
                        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)]
                                String[] valuePaths,
                        [MarshalAs(UnmanagedType.I4)]EvtRenderContextFlags flags
                                );

    [DllImport("wevtapi.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtRender(
                        IntPtr context,
                        IntPtr eventHandle,
                        EvtRenderFlags flags,
                        int buffSize,
                        [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder buffer,
                        out int buffUsed,
                        out int propCount
                                    );


    [DllImport("wevtapi.dll", EntryPoint = "EvtRender", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtRender(
                        IntPtr context,
                        IntPtr eventHandle,
                        EvtRenderFlags flags,
                        int buffSize,
                        IntPtr buffer,
                        out int buffUsed,
                        out int propCount
                                    );


    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Auto)]
    public struct EvtStringVariant
    {
        [MarshalAs(UnmanagedType.LPWStr), FieldOffset(0)]
        public string StringVal;
        [FieldOffset(8)]
        public UInt32 Count;
        [FieldOffset(12)]
        public UInt32 Type;
    };

    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtFormatMessage(
                         IntPtr publisherMetadataHandle,
                         IntPtr eventHandle,
                         uint messageId,
                         int valueCount,
                         EvtStringVariant[] values,
                         [MarshalAs(UnmanagedType.I4)]EvtFormatMessageFlags flags,
                         int bufferSize,
                         [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder buffer,
                         out int bufferUsed
                                    );

    [DllImport("wevtapi.dll", EntryPoint = "EvtFormatMessage", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtFormatMessageBuffer(
                         IntPtr publisherMetadataHandle,
                         IntPtr eventHandle,
                         uint messageId,
                         int valueCount,
                         IntPtr values,
                         [MarshalAs(UnmanagedType.I4)]EvtFormatMessageFlags flags,
                         int bufferSize,
                         IntPtr buffer,
                         out int bufferUsed
                                    );

    //SESSION
    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr EvtOpenSession(
                        [MarshalAs(UnmanagedType.I4)]EvtLoginClass loginClass,
                        ref EvtRpcLogin login,
                        int timeout,
                        int flags
                                    );

    //BOOKMARK
    [DllImport("wevtapi.dll", EntryPoint = "EvtCreateBookmark", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr EvtCreateBookmark(
                        [MarshalAs(UnmanagedType.LPWStr)] string bookmarkXml
                                    );

    [DllImport("wevtapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EvtUpdateBookmark(
                        IntPtr bookmark,
                        IntPtr eventHandle
                                    );
    //
    // EventLog
    //

}

