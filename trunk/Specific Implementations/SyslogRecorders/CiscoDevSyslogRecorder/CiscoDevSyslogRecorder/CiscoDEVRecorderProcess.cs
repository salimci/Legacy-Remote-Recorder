using System;
using System.Collections.Generic;
using System.Text;
using Log;
using LogMgr;
using CustomTools; 

namespace CiscoDevSyslogRecorder
{
    public class CiscoDEVRecorderProcess
    {
        #region Deðiþkenler
                private string sourceName;
                private int sourceportNumber;
                private int sequenceNo;
                private string dateTime;
                private string facility;
                private int severity;
                private string mnemonic;
                private string messageText;
                private string logName;
                private string eventType;
        #endregion

        public CiscoDEVRecorderProcess() 
        {
            sourceName = null;
            sourceportNumber = 0;
            sequenceNo = 0;
            dateTime = null;
            facility = null;
            severity = 0; 
            mnemonic = null; 
            messageText = null; 
            logName = null; 
            eventType = null; 
        }

        public void parsingProcess(LogMgrEventArgs args, int zone)
        {
            this.logName = "Cisco DEV Recorder";
            InitializeLogger.L.Log(LogType.FILE, LogLevel.DEBUG, "Message" + args.Message);
            this.eventType = args.EventLogEntType.ToString();
            this.messageText = args.Message.Replace('\0', ' ');
            string[] syslogMessageArr = args.Message.Split(':');

            for (int i = 0; i < syslogMessageArr.Length; i++)
            {
                syslogMessageArr[i] = syslogMessageArr[i].Trim();
            }

            if (syslogMessageArr.Length == 8)
            {
                #region parser

                try
                {
                    sourceName = syslogMessageArr[0];
                }
                catch (Exception ex)
                {
                    InitializeLogger.L.Log(LogType.FILE, LogLevel.ERROR, "Couldnt find sourceName :" + ex.Message);
                }

                try
                {
                    sourceportNumber = Convert.ToInt32(syslogMessageArr[1]);
                }
                catch (Exception ex)
                {
                    InitializeLogger.L.Log(LogType.FILE, LogLevel.ERROR, "Couldnt find sourceportNumber :" + ex.Message);
                }

                try
                {
                    sequenceNo = Convert.ToInt32(syslogMessageArr[2].Split(' ')[1]);
                }
                catch (Exception ex)
                {
                    InitializeLogger.L.Log(LogType.FILE, LogLevel.ERROR, "Couldnt find sequence no :" + ex.Message);
                }

                try
                {
                    string[] date ={ "", "", "", "" };

                    date[0] = Convert.ToString(DateTime.Now.Year);

                    string[] datepartvirtual = syslogMessageArr[3].Split(' ');
                    int count = 0;

                    for (int i = 0; i < datepartvirtual.Length; i++)
                    {
                        if (datepartvirtual[i] == "")
                        {
                            count++;
                        }
                    }

                    string[] datepart = new string[datepartvirtual.Length - count];
                    int k = 0;
                    for (int j = 0; j < datepartvirtual.Length; j++)
                    {
                        if (datepartvirtual[j] != "")
                        {
                            datepart[k] = datepartvirtual[j];
                            k++;
                        }
                    }

                    date[1] = datepart[0].TrimStart('*');
                    date[2] = datepart[1];
                    date[3] = datepart[2] + ":" + syslogMessageArr[4] + ":" + syslogMessageArr[5];

                    string logDate = "";
                    for (int i = 0; i < 4; i++)
                    {
                        logDate += date[i] + " ";
                    }

                    DateTime _logDate = new DateTime();
                    _logDate = Convert.ToDateTime(logDate.TrimEnd());
                    this.dateTime = _logDate.AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss"); // Date Time
                }
                catch (Exception ex)
                {
                    InitializeLogger.L.Log(LogType.FILE, LogLevel.ERROR, "An error occured while parsing date time  :" + ex.Message);
                }

                try
                {
                    facility = syslogMessageArr[6].Split('-')[0].TrimStart('%');
                }
                catch (Exception ex)
                {
                    InitializeLogger.L.Log(LogType.FILE, LogLevel.ERROR, "An error occured while parsing facility  :" + ex.Message);
                }

                try
                {
                    severity = Convert.ToInt32(syslogMessageArr[6].Split('-')[1]);
                }
                catch (Exception ex)
                {
                    InitializeLogger.L.Log(LogType.FILE, LogLevel.ERROR, "An error occured while parsing severity  :" + ex.Message);
                }

                try
                {
                    mnemonic = syslogMessageArr[6].Split('-')[2];
                }
                catch (Exception ex)
                {
                    InitializeLogger.L.Log(LogType.FILE, LogLevel.ERROR, "An error occured while parsing mnemonic  :" + ex.Message);
                }

                try
                {
                    messageText = syslogMessageArr[7];
                }
                catch (Exception ex)
                {
                    InitializeLogger.L.Log(LogType.FILE, LogLevel.ERROR, "An error occured while parsing messageText  :" + ex.Message);
                }
                #endregion
            }
            else 
            {
                InitializeLogger.L.Log(LogType.FILE, LogLevel.ERROR, "Unexcepted log format");
            }
        }

        public CustomBase.Rec createRec() 
        {
                 CustomBase.Rec rec = new CustomBase.Rec();
                 
                 rec.SourceName = sourceName;
                 rec.Datetime = dateTime;
                 rec.LogName = logName;
                 rec.EventType = eventType;
                 
                 rec.CustomInt1 = sequenceNo;
                 rec.CustomInt2 = severity;
                 rec.CustomInt9 = Convert.ToInt64(sourceportNumber);
                 
                 rec.CustomStr1 = facility;
                 rec.CustomStr2 = mnemonic;
                 rec.Description = messageText;
                 
                 return rec;
        }        
    }   
}

 