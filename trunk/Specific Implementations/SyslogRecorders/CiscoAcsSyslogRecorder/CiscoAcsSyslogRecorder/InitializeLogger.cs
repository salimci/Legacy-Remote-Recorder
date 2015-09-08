using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using Log;

namespace CiscoACSRecorder
{
    class InitializeLogger
    {        
           private static CLogger l = null;
           private RegistryProcess rp = new RegistryProcess();
           private uint logging_interval;
           private uint log_size;
           private int trc_level;
             
           public InitializeLogger()
           {
                 logging_interval = 60000;
                 log_size = 1000000;
                 l = new CLogger();
                 trc_level = 4;
           }
           
           public InitializeLogger(int trc_Level)
           {
               logging_interval = 60000;
               log_size = 1000000;
               l = new CLogger();
               trc_level = trc_Level;
           } 
           
           public InitializeLogger(uint interval, uint size, int trc_Level)
           {
               logging_interval = interval;
               log_size = size;
               l = new CLogger();
               trc_level = trc_Level;
           }
           
           public uint LOGGING_INTERVAL 
           {

                get 
                {
                    return logging_interval;
                }
           }
            
           public uint LOG_SIZE
           {
                get
                {
                    return log_size;
                }
           }
           
           public int TRC_LEVEL
           {
               get
               {
                    return trc_level;
               }
           }
            
           public bool Initialize(RegistryProcess rp) 
           {
               try
               {
                   switch (trc_level)
                   {
                       case 0:
                           {
                               l.SetLogLevel(LogLevel.NONE);
                           } break;
                       case 1:
                           {
                               l.SetLogLevel(LogLevel.INFORM);
                           } break;
                       case 2:
                           {
                               l.SetLogLevel(LogLevel.WARN);
                           } break;
                       case 3:
                           {
                               l.SetLogLevel(LogLevel.ERROR);
                           } break;
                       case 4:
                           {
                               l.SetLogLevel(LogLevel.DEBUG);
                           } break;
                   }
                   
                   l.SetLogFile(rp.ERR_LOG);
                   l.SetTimerInterval(LogType.FILE, logging_interval);
                   l.SetLogFileSize(log_size);
                   l.Log(LogType.FILE, LogLevel.INFORM, "Logger is initialized");
                   EventLog.WriteEntry("Security Manager Syslog Recorder", "Logger is initialized", EventLogEntryType.Information);                   
                   return true;
               }
               catch (Exception er)
               {
                   EventLog.WriteEntry("Security Manager Syslog Recorder", er.ToString(), EventLogEntryType.Error);
                   return false;   
               }
           }
           
           public static CLogger L 
           {
                 get
                 {
                     return l;
                 }
           }
    }
}
