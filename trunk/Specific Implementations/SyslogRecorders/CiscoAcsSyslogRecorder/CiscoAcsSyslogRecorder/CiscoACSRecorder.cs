using System;
using System.Collections.Generic;
using System.Text;
using Log;
using LogMgr;
using CustomTools; 

namespace CiscoACSRecorder
{
    public class CiscoACSRecorderProcess
    {
        #region Deðiþkenler
            private string sourceName;
            private int sourceportNumber;
            private string dateTime;
            private string logType;
            private string message_Id;
            private string userName;
            private string nas_ip_Address;
            private int nas_port;
            private string groupName;
            private string framed_ip_Address;
            private string calling_station_Id;
            private string acct_status_Type;
            private string acct_session_Id;
            private string group_Name;
            private string nas_Portname;
            private string caller_Id;
            private string acct_Flags;
            private string service;
            private int task_Id;
            private string aaa_Server;
            private string message_Type;
            private string filter_Information;
            private string access_Device;
            private string authen_failure_Code;
            private string status_Class;
            private string text_Message;
            private string log_Name;
            private string event_Type;
            private string description;
            private int system_Memory_Usage;
            private int system_Free_Disk_Space;
            private int system_Cpu_Usage;
            private string cmd;
            private int priv_lvl;
            private string unexpectedDescription;
        #endregion

        public CiscoACSRecorderProcess() 
        { 
             sourceName = null; 
             sourceportNumber = 0;
             dateTime = null;
             logType = null;
             message_Id = null;
             userName = null;
             nas_ip_Address = null;
             nas_port = 0;
             groupName = null;
             framed_ip_Address = null;
             calling_station_Id = null;
             acct_status_Type = null;
             acct_session_Id = null;
             group_Name = null;
             nas_Portname = null;
             caller_Id = null;
             acct_Flags = null;
             service = null;
             task_Id = 0;
             aaa_Server = null;
             message_Type = null;
             filter_Information = null;
             access_Device = null;
             authen_failure_Code = null;
             status_Class = null;
             text_Message = null;
             log_Name = null;
             event_Type = null;
             description = null;
             system_Memory_Usage = 0;
             system_Free_Disk_Space = 0;
             system_Cpu_Usage = 0;
             cmd = null;
             priv_lvl = 0;
             unexpectedDescription = null;
        }

        public void parsingProcess(LogMgrEventArgs args, int zone)
        {
            string[] logproperties = {"User-Name", "NAS-IP-Address", "NAS-Port", "Group-Name", "Framed-IP-Address", "Calling-Station-Id", "Acct-Status-Type", 
                                       "Acct-Session-Id", "NAS-Portname", "Caller-Id", "Acct-Flags", "service", "task_id", "AAA Server", 
                                       "Message-Type", "Filter Information", "Access Device", "Message-Type", "Authen-Failure-Code", 
                                       "status-class", "text-message", "system-memory-usage", "system-free-disk-space", "System-CPU-usage", 
                                       "action-type","cmd","priv-lvl","Caller-ID"};
            
            this.log_Name = "Cisco ACS Recorder";
            InitializeLogger.L.Log(LogType.FILE, LogLevel.DEBUG, "Message" + args.Message);
            this.event_Type = args.EventLogEntType.ToString();
            this.description = args.Message.Replace('\0', ' ');
            string[] syslogMessageArr = args.Message.Split(',');
            
            string[] _syslogmessageArrIndex = syslogMessageArr[0].Split(' ');
            int count = 0;

            for (int i = 0; i < _syslogmessageArrIndex.Length; i++)
            {
                if (_syslogmessageArrIndex[i] == "") 
                {
                    count++; 
                }   
            }
            
            string[] syslogmessageArrIndex0 = new string[_syslogmessageArrIndex.Length - count];

            int indexa = 0;

            for (int i = 0; i < _syslogmessageArrIndex.Length; i++)
            {
                if (_syslogmessageArrIndex[i] != "")
                {
                    syslogmessageArrIndex0[indexa] = _syslogmessageArrIndex[i];
                    indexa++;
                }
            }
            
            try
            {
                String[] sourceArr = syslogmessageArrIndex0[0].Split(':');
                this.sourceName = sourceArr[0]; //Source Name
                this.sourceportNumber = Convert.ToInt32(sourceArr[1]); //Source Port
            }
            catch (Exception e)
            {
                InitializeLogger.L.Log(LogType.FILE, LogLevel.ERROR, "Couldnt find source port number :" + e.Message);
            }

            this.logType = syslogmessageArrIndex0[7];

            string[] date ={ "", "", "", "" };
            date[0] = Convert.ToString(DateTime.Now.Year);
            date[1] = syslogmessageArrIndex0[3];
            date[2] = syslogmessageArrIndex0[4];
            date[3] = syslogmessageArrIndex0[5];

            string logDate = "";
            for (int i = 0; i < 4; i++)
            {
                logDate += date[i] + " ";
            }

            DateTime _logDate = new DateTime();
            _logDate = Convert.ToDateTime(logDate.TrimEnd());
            this.dateTime = _logDate.AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss"); // Date Time

            this.message_Id = syslogmessageArrIndex0[8];
            int index;
            string property="";

            try
            {
                bool kontrol = true;
                if (syslogmessageArrIndex0[11].Contains("="))
                {
                    property = syslogmessageArrIndex0[11].Split('=')[0];
                }
                else 
                {
                    if (syslogmessageArrIndex0[11] == "AAA")
                    {
                        property = "AAA Server";
                    }
                    kontrol = false;
                }

                
                index = Array.IndexOf(logproperties, property);

                if (kontrol)
                {
                    assignpropertyvalue(index, syslogmessageArrIndex0[11].Split('=')[1]);
                }
                else 
                {
                    assignpropertyvalue(index, syslogmessageArrIndex0[12].Split('=')[1]);
                }
            }
            catch (Exception e)
            {
                InitializeLogger.L.Log(LogType.FILE, LogLevel.ERROR, "error on parsing the AAA Server :" + e.Message);
            }
            
            for (int i = 1; i < syslogMessageArr.Length-1; i++)
            {
                index = -1;
                property ="";
                property = syslogMessageArr[i].Split('=')[0];
                index = Array.IndexOf(logproperties,property);
                if(index != -1)
                {
                    assignpropertyvalue(index, syslogMessageArr[i].Split('=')[1]); 
                }
                else
                {
                    assignundefinedvalue(syslogMessageArr[i].Split('=')[0], syslogMessageArr[i].Split('=')[1]);
                }
            }
        }

        private void assignundefinedvalue(string property, string value)
        {
            if (unexpectedDescription == null) 
            {
                unexpectedDescription = "";
            }

            if (unexpectedDescription == "")
            {
                unexpectedDescription += property + "=" + value;
            }
            else
            {
                unexpectedDescription += "," + property + "=" + value +",";
            }
        }
        
        private void assignpropertyvalue(int i, string virtualsyslogMessageArr) 
        {
            switch (i)
            {
                case 0:
                    {
                        userName = virtualsyslogMessageArr;
                    } break;
                case 1:
                    {
                        nas_ip_Address = virtualsyslogMessageArr;
                    } break;
                case 2:
                    {
                        Type t = virtualsyslogMessageArr.GetType();
                        if (t.ToString() == "System.Int32")
                        {
                            nas_port = Convert.ToInt32(virtualsyslogMessageArr);
                        }
                        if (t.ToString() == "System.String")
                        {
                            nas_Portname = virtualsyslogMessageArr;
                        }
                    } break;
                case 3:
                    {
                        group_Name = virtualsyslogMessageArr;
                    } break;
                case 4:
                    {
                        framed_ip_Address = virtualsyslogMessageArr;
                    } break;
                case 5:
                    {
                        calling_station_Id = virtualsyslogMessageArr;
                    } break;
                case 6:
                    {
                        acct_status_Type = virtualsyslogMessageArr;
                    } break;
                case 7:
                    {
                        acct_session_Id = virtualsyslogMessageArr;
                    } break;
                case 8:
                    {
                        nas_Portname = virtualsyslogMessageArr;
                    } break;
                case 9:
                    {
                        caller_Id = virtualsyslogMessageArr;
                    } break;
                case 10:
                    {
                        acct_Flags = virtualsyslogMessageArr;
                    } break;
                case 11:
                    {
                        service = virtualsyslogMessageArr;
                    } break;
                case 12:
                    {
                        task_Id = Convert.ToInt32(virtualsyslogMessageArr);
                    } break;
                case 13:
                    {
                        aaa_Server = virtualsyslogMessageArr;
                    } break;
                case 14:
                    {
                        message_Type = virtualsyslogMessageArr;
                    } break;
                case 15:
                    {
                        filter_Information = virtualsyslogMessageArr;
                    } break;
                case 16:
                    {
                        access_Device = virtualsyslogMessageArr;
                    } break;
                case 17:
                    {
                        message_Type = virtualsyslogMessageArr;
                    } break;
                case 18:
                    {
                        authen_failure_Code = virtualsyslogMessageArr;
                    } break;
                case 19:
                    {
                        status_Class = virtualsyslogMessageArr;
                    } break;
                case 20:
                    {
                        text_Message = virtualsyslogMessageArr;
                    } break;
                case 21:
                    {
                        system_Memory_Usage = Convert.ToInt32(virtualsyslogMessageArr);
                    } break;
                case 22:
                    {
                        system_Free_Disk_Space = Convert.ToInt32(virtualsyslogMessageArr);
                    } break;
                case 23:
                    {
                        system_Cpu_Usage = Convert.ToInt32(virtualsyslogMessageArr);
                    } break;
                case 24:
                    {
                        acct_status_Type = virtualsyslogMessageArr;
                    } break;
                case 25:
                    {
                        cmd = virtualsyslogMessageArr;
                    } break;
                case 26:
                    {
                        priv_lvl = Convert.ToInt32(virtualsyslogMessageArr);
                    } break;
                case 27:
                    {
                        caller_Id = virtualsyslogMessageArr;
                    } break;
            }
        }

        public CustomBase.Rec createRec() 
        {
                 CustomBase.Rec rec = new CustomBase.Rec();
                 
                 rec.SourceName = sourceName;
                 rec.Datetime = dateTime;
                 rec.UserName = userName;
                 rec.LogName = log_Name;
                 rec.EventType = event_Type;
                 
                 rec.CustomInt1 = nas_port;
                 rec.CustomInt2 = task_Id;
                 rec.CustomInt3 = system_Memory_Usage;
                 rec.CustomInt4 = system_Free_Disk_Space;
                 rec.CustomInt5 = system_Cpu_Usage;
                 rec.CustomInt6 = Convert.ToInt64(priv_lvl);
                 rec.CustomInt9 = Convert.ToInt64(sourceportNumber);

                 rec.CustomStr1 = acct_session_Id;
                 rec.CustomStr2 = group_Name;
                 rec.CustomStr3 = calling_station_Id;
                 rec.CustomStr4 = framed_ip_Address;
                 rec.CustomStr5 = nas_ip_Address;
                 rec.CustomStr6 = acct_status_Type;
                 rec.CustomStr7 = logType;
                 rec.CustomStr8 = authen_failure_Code;
                 rec.CustomStr9 = caller_Id;
                 rec.CustomStr10 = message_Id;
                 
                 rec.Description = createDescription();
                 
                 return rec;
        }        

        private string createDescription() 
        {
            string desc = "";
            
            if(acct_Flags != null)
            {
                desc += "Acct - Flags=" + acct_Flags+",";
            }
            if (service != null)
            {
                desc += "service=" + service + ","; 
            }
            if (aaa_Server != null)
            {
                desc += "AAA Server=" + aaa_Server + ","; 
            }
            if (message_Type != null)
            {
                desc += "Message-Type=" + message_Type + ","; 
            }
            if (filter_Information != null)
            {
                desc += "Filter Information=" + filter_Information + ","; 
            }
            if (access_Device != null)
            {
                desc += "Access Device=" + access_Device + ","; 
            }
            if (nas_Portname != null)
            {
                desc += "NAS-Portname=" + nas_Portname + ","; 
            }
            if (status_Class != null)
            {
                desc += "status-class=" + status_Class + ","; 
            }
            if (text_Message != null)
            {
                desc += "text-message=" + text_Message + ","; 
            }
            if (cmd != null)
            {
                desc += "cmd=" + cmd + ","; 
            }
            desc = desc.TrimEnd(',');

            if (unexpectedDescription != null && unexpectedDescription != "") 
            {
                unexpectedDescription = unexpectedDescription.TrimEnd(',');
                desc += "UNEXPECTED PROPERTY NAMES & VALUES = " + unexpectedDescription; 
            }

            return desc;
        }
    }
}

 