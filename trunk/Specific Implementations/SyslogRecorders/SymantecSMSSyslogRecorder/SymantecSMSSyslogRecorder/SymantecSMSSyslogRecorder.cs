using System;
using System.Collections.Generic;
using System.Text;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.ServiceProcess;
using System.Net;
using System.Net.Sockets;

namespace SymantecSmsSyslogRecorder
{
    public class SymantecSmsSyslogRecorder : CustomBase
    {
        #region "Private variables"

        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 3, Syslog_Port = 514,zone=0;
        private string err_log, protocol = "UDP", location = "", remote_host = "localhost";
        private CLogger L;
        public Syslog slog = null;
        private bool reg_flag = false;
        protected bool usingRegistry = false;
        private ProtocolType pro;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;

        #endregion

        private void InitializeComponent()
        {
        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
        String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
        String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
        String CustomVar1, int CustomVar2, String Virtualhost, String dal,Int32 Zone)
        {
            usingRegistry = false;
            Id = Identity;
            location = Location;
            //last_position = LastPosition;
            //fromend = FromEndOnLoss;
            //max_record_send = MaxLineToWait;
            //timer_interval = SleepTime;
            //user = User;
            //password = Password;
            //remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            Dal = dal;
            remote_host = RemoteHost;
            zone = Zone;
        }
        public override void Clear()
        {
            if (slog != null)
                slog.Stop();
        }

        public override void Init()
        {
            try
            {
                if (usingRegistry)
                {
                    if (!reg_flag)
                    {
                        if (!Read_Registry())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                            return;
                        }
                        else
                            if (!Initialize_Logger())
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SymantecSmsSyslog Recorder functions may not be running");
                                return;
                            }
                        reg_flag = true;
                    }
                }
                else
                {
                    if (!reg_flag)
                    {
                        if (!Get_logDir())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Getting the log dir");
                            return;
                        }
                        else
                            if (!Initialize_Logger())
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SymantecSmsSyslog Recorder functions may not be running");
                                return;
                            }
                        reg_flag = true;
                    }

                    if (location.Length > 1)
                    {
                        if (location.Contains(':'.ToString()))
                        {
                            protocol = location.Split(':')[0];
                            Syslog_Port = Convert.ToInt32(location.Split(':')[1]);
                            if (protocol.ToLower() == "tcp")
                                pro = ProtocolType.Tcp;
                            else
                                pro = ProtocolType.Udp;
                        }
                        else
                        {
                            protocol = location;
                            Syslog_Port = 514;
                        }
                    }
                    else
                    {
                        pro = ProtocolType.Udp;
                        Syslog_Port = 514;
                    }
                }
                //L.Log(LogType.FILE, LogLevel.INFORM, "Start listening SymantecSmsSyslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());
                //slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port, pro);

                if (usingRegistry)
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "Start listening Syslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());
                    slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port, pro);
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "Start listening Syslogs on ip: " + remote_host + " port: " + Syslog_Port.ToString());
                    slog = new Syslog(remote_host, Syslog_Port, pro);
                }

                slog.Start();
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(slog_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing SymantecSmsSyslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SymantecSmsSyslog Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\SymantecSmsSyslogRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SymantecSmsSyslog Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public SymantecSmsSyslogRecorder()
        {
        }

        public String[] SpaceSplit(String line)
        {
            List<String> lst = new List<String>();
            StringBuilder sb = new StringBuilder();
            bool space = false;
            foreach (Char c in line.ToCharArray())
            {
                if (c != ' ')
                {
                    if (space)
                    {
                        if (sb.ToString() != "")
                        {
                            lst.Add(sb.ToString());
                            sb.Remove(0, sb.Length);
                        }
                        space = false;
                    }
                    sb.Append(c);
                }
                else if (!space)
                {
                    space = true;
                }
            }

            if (sb.ToString() != "")
                lst.Add(sb.ToString());

            return lst.ToArray();
        }

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");

                rec.LogName = "SymantecSmsSyslog Recorder";

                rec.EventCategory = "sms";
                rec.UserName = "GENERAL";

                rec.EventType = args.EventLogEntType.ToString();

                if (args.Message == "")
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "Message is null.");
                    return;
                }

                String[] Desc = args.Message.Split(':');

                if (Desc.Length < 5)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Length of message too small: " + args.Message);
                    return;
                }

                for (Int32 i = 0; i < Desc.Length; ++i)
                {
                    Desc[i] = Desc[i].Trim();
                }

                rec.ComputerName = Desc[0] + ":" + Desc[1];
                rec.SourceName = args.Source;

                String[] dateArr = SpaceSplit(Desc[2].TrimStart(rec.SourceName.ToCharArray()));

                if (dateArr.Length < 3)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message for datetime (text too small): " + args.Message);
                    return;
                }

                try
                {
                    StringBuilder dateString = new StringBuilder();
                    //Date
                    dateString.Append(dateArr[0]).Append(" ").Append(dateArr[1]).Append(" ").Append(DateTime.Now.Year.ToString()).Append(" ");
                    //Time
                    dateString.Append(dateArr[2]).Append(":").Append(Desc[3]).Append(":").Append(Desc[4].Substring(0, 2));
                    DateTime dt = DateTime.Parse(dateString.ToString());
                    rec.Datetime = dt.AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                }
                catch (Exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing datetime text: " + args.Message);
                    return;
                }

                try
                {
                    string codeText = Desc[4].Substring(2).Trim().TrimStart(rec.EventCategory.ToCharArray()).Trim();
                    if (codeText.Contains("[") && codeText.Contains("]"))
                    {
                        rec.CustomStr1 = codeText.Split('[')[0].Trim();
                        rec.CustomInt1 = int.Parse(codeText.Split('[')[1].Trim().Split(']')[0].Trim());
                    }
                    else
                    {
                        rec.CustomStr1 = Desc[4].Substring(2).Trim(); //.TrimStart(rec.EventCategory.ToCharArray()).Trim(); //codeText;
                        rec.CustomInt1 = 0;
                    }
                }
                catch (Exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing code text: " + args.Message);
                    return;
                }

                int lastIndexForDesc = 5;
                if (Desc.Length > 5)
                {
                    if (Desc[5].Contains("ML-HOST_DISCONNECTED"))
                    {
                        try
                        {
                            rec.UserName = "GENERAL";
                            rec.CustomInt2 = int.Parse(Desc[5].Split(']')[0].TrimStart('[').Trim());
                            rec.EventCategory = "ML-HOST_DISCONNECTED";
                            if (Desc[7].ToLower().Contains("disconnected"))
                            {
                                rec.CustomStr10 = Desc[6] + ":" + Desc[7].Split(' ')[0]; //disconnected from
                            }
                            lastIndexForDesc = 6;
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Unknown format for ML-HOST_DISCONNECTED: " + args.Message + " \nEx: " + ex.Message);
                            return;
                        }
                    }
                    else if (Desc[5].Contains("ML-HOST_CONNECTED"))
                    {
                        try
                        {
                            rec.UserName = "GENERAL";
                            rec.CustomInt2 = int.Parse(Desc[5].Split(']')[0].TrimStart('[').Trim());
                            rec.EventCategory = "ML-HOST_CONNECTED";
                            if (Desc[7].ToLower().Contains("connected"))
                            {
                                rec.CustomStr10 = Desc[6] + ":" + Desc[7].Split(' ')[0]; //connected to
                            }
                            lastIndexForDesc = 6;
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Unknown format for ML-HOST_CONNECTED: " + args.Message + " \nEx: " + ex.Message);
                            return;
                        }
                    }
                    else if (Desc[5].Contains("ML-RECEIVED"))
                    {
                        //195.142.175.69:62754 : mail.info Jul 29 15:19:23 mail ecelerity: [18796] ML-RECEIVED_RECIPIENT: Message ID: E0/0C-18796-B45A23E4, Audit ID: c0a8010e-b7bc5ae00000496c-57-4e32a54bb662, recipient: Mehmet.Gulcen@ingbank.com.tr
                        try
                        {
                            rec.UserName = "GENERAL";
                            rec.CustomInt2 = int.Parse(Desc[5].Split(']')[0].TrimStart('[').Trim());
                            rec.EventCategory = "ML-RECEIVED";

                            if (Desc[6].Contains("Message ID") && Desc[7].Contains("Audit ID"))
                            {
                                try
                                {
                                    rec.CustomStr2 = Desc[8] + ":" + Desc[9].TrimEnd(", from host".ToCharArray()); //Received on
                                    if (Desc.Length > 10)
                                    {
                                        rec.CustomStr10 = Desc[10] + ":" + Desc[11].TrimEnd(", sender".ToCharArray()); //from host
                                        rec.CustomStr4 = Desc[12].Split(',')[0]; //sender

                                        rec.CustomInt3 = int.Parse(Desc[13].TrimEnd(", Note".ToCharArray())); //size
                                    }
                                }
                                catch (Exception ex)
                                {
                                    rec.CustomStr2 = Desc[8].TrimEnd(", from host".ToCharArray()); //Received on
                                    rec.CustomStr10 = Desc[9].TrimEnd(", sender".ToCharArray()); //from host
                                    rec.CustomStr4 = Desc[10].Split(',')[0]; //sender

                                    rec.CustomInt3 = int.Parse(Desc[11].TrimEnd(", Note".ToCharArray())); //size
                                }
                                finally
                                {
                                    rec.CustomStr5 = Desc[7].Split(',')[0]; //Message ID
                                    rec.CustomStr6 = Desc[7].Split(',')[1].TrimStart("Audit ID".ToCharArray()).Trim(); //Audit ID
                                }
                            }
                            lastIndexForDesc = 6;
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Unknown format for ML-RECEIVED: " + args.Message + " \nEx: " + ex.Message);
                            return;
                        }
                    }
                    else if (Desc[5].Contains("ML-REJECT"))
                    {
                        try
                        {
                            rec.UserName = "GENERAL";
                            rec.CustomInt2 = int.Parse(Desc[5].Split(']')[0].TrimStart('[').Trim());
                            rec.EventCategory = "ML-REJECT";

                            if (Desc[6].Contains("Rejection") && Desc[10].Contains("Audit ID"))
                            {
                                rec.CustomStr2 = Desc[7] + ":" + Desc[8].TrimEnd(", sent to host".ToCharArray()); //Rejection on
                                rec.CustomStr10 = Desc[9] + ":" + Desc[10].Split(',')[0] + (Desc[10].Split(',')[1].Contains("Audit ID") ? "" : Desc[10].Split(',')[1]); //sent to host
                                rec.CustomStr6 = Desc[10].Split(',')[1].Contains("Audit ID") ? Desc[10].Split(',')[1].TrimStart("Audit ID".ToCharArray()).Trim() : Desc[10].Split(',')[2].TrimStart("Audit ID".ToCharArray()).Trim(); //Audit ID
                            }
                            lastIndexForDesc = 6;
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Unknown format for ML-REJECT: " + args.Message + " \nEx: " + ex.Message);
                            return;
                        }
                    }
                    else if (Desc[5].Contains("ML-DELIVERY_ATTEMPT"))
                    {
                        try
                        {
                            rec.UserName = "GENERAL";
                            rec.CustomInt2 = int.Parse(Desc[5].Split(']')[0].TrimStart('[').Trim());
                            rec.EventCategory = "ML-DELIVERY_ATTEMPT";

                            if (Desc[6].Contains("Message ID") && Desc[7].Contains("Audit ID"))
                            {
                                rec.CustomStr4 = Desc[8]; //sender

                                rec.CustomStr5 = Desc[7].Split(',')[0]; //Message ID
                                rec.CustomStr6 = Desc[7].Split(',')[1].TrimStart("Audit ID".ToCharArray()).Trim(); //Audit ID
                            }
                            lastIndexForDesc = 6;
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Unknown format for ML-DELIVERY_ATTEMPT: " + args.Message + " \nEx: " + ex.Message);
                            return;
                        }
                    }
                    else if (Desc[5].Contains("ML-DELIVERY"))
                    {
                        try
                        {
                            rec.UserName = "GENERAL";
                            rec.CustomInt2 = int.Parse(Desc[5].Split(']')[0].TrimStart('[').Trim());
                            rec.EventCategory = "ML-DELIVERY";

                            if (Desc[6].Contains("Message ID") && Desc[7].Contains("Audit ID"))
                            {
                                rec.CustomStr10 = Desc[8].TrimEnd(", sender".ToCharArray()); //Delivery succeeded to host
                                rec.CustomStr4 = Desc[9].TrimEnd(", Note".ToCharArray()); //sender

                                rec.CustomStr5 = Desc[7].Split(',')[0]; //Message ID
                                rec.CustomStr6 = Desc[7].Split(',')[1].TrimStart("Audit ID".ToCharArray()).Trim(); //Audit ID
                            }
                            lastIndexForDesc = 6;
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Unknown format for ML-DELIVERY: " + args.Message + " \nEx: " + ex.Message);
                            return;
                        }
                    }
                    else
                    {
                        if (Desc[5].Contains("|SOURCE|"))
                        {
                            try
                            {
                                rec.UserName = "MAILLOG_SOURCE";
                                string[] descText = Desc[5].Split('|');
                                rec.EventCategory = "MESSAGE SOURCE";
                                rec.CustomStr5 = descText[0]; //Message ID
                                rec.CustomStr6 = descText[1]; //Audit ID
                                rec.CustomStr2 = descText[3]; //Mail Source (internal / external)
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Unknown format for message SOURCE: " + args.Message + " \nEx: " + ex.Message);
                                return;
                            }
                        }
                        else if (Desc[5].Contains("|ACCEPT|"))
                        {
                            try
                            {
                                rec.UserName = "MAILLOG_ACCEPT";
                                string[] descText = Desc[5].Split('|');
                                rec.EventCategory = "MESSAGE ACCEPT";
                                rec.CustomStr5 = descText[0]; //Message ID
                                rec.CustomStr6 = descText[1]; //Audit ID
                                rec.CustomStr2 = descText[3] + ":" + Desc[6]; // Mail Server IP Address
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Unknown format for message ACCEPT: " + args.Message + " \nEx: " + ex.Message);
                                return;
                            }
                        }
                        else if (Desc[5].Contains("|SUBJECT|"))
                        {
                            try
                            {
                                rec.UserName = "MAILLOG_SUBJECT";
                                string[] descText = Desc[5].Split('|');
                                rec.EventCategory = "MESSAGE SUBJECT";
                                rec.CustomStr5 = descText[0]; //Message ID
                                rec.CustomStr6 = descText[1]; //Audit ID
                                rec.CustomStr8 = descText[3]; // Subject Text
                                for (int i = 6; i < Desc.Length; i++)
                                {
                                    rec.CustomStr8 += ":" + Desc[i];
                                }
                                if (rec.CustomStr8.Length > 900)
                                {
                                    rec.CustomStr8 = rec.CustomStr8.Substring(0, 895) + "...";
                                    L.Log(LogType.FILE, LogLevel.INFORM, "Subject length too long. Only 895 characters taken..");
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Unknown format for message SUBJECT: " + args.Message + " \nEx: " + ex.Message);
                                return;
                            }
                        }
                        else if (Desc[5].Contains("|VERDICT|"))
                        {
                            try
                            {
                                rec.UserName = "MAILLOG_VERDICT";
                                string[] descText = Desc[5].Split('|');
                                rec.EventCategory = "MESSAGE VERDICT";
                                rec.CustomStr5 = descText[0]; //Message ID
                                rec.CustomStr6 = descText[1]; //Audit ID
                                rec.CustomStr2 = descText[3]; // Mail address

                                for (int i = 4; i < descText.Length; i++)
                                {
                                    if (descText[i].Contains("@"))
                                    {
                                        continue;
                                    }
                                    rec.CustomStr3 += descText[i] + "/";
                                }
                                rec.CustomStr3 = rec.CustomStr3.TrimEnd("/".ToCharArray()); // Verdict Text
                                if (rec.CustomStr3.Length > 900)
                                {
                                    rec.CustomStr3 = rec.CustomStr3.Substring(0, 895) + "...";
                                    L.Log(LogType.FILE, LogLevel.INFORM, "Verdict length too long. Only 895 characters taken..");
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Unknown format for message VERDICT: " + args.Message + " \nEx: " + ex.Message);
                                return;
                            }
                        }
                        else if (Desc[5].Contains("|IRCPTACTION|"))
                        {
                            try
                            {
                                rec.UserName = "MAILLOG_IRCPTACTION";
                                string[] descText = Desc[5].Split('|');
                                rec.EventCategory = "MESSAGE IRCPTACTION";
                                rec.CustomStr5 = descText[0]; //Message ID
                                rec.CustomStr6 = descText[1]; //Audit ID
                                for (int i = 3; i < descText.Length - 1; i++)
                                {
                                    rec.CustomStr2 += descText[i] + ",";
                                }
                                rec.CustomStr2 = rec.CustomStr2.TrimEnd(",".ToCharArray()); // Recipient Addresses
                                rec.CustomStr3 = descText[descText.Length - 1]; // Action
                                if (rec.CustomStr3.Length > 900)
                                {
                                    rec.CustomStr3 = rec.CustomStr3.Substring(0, 895) + "...";
                                    L.Log(LogType.FILE, LogLevel.INFORM, "Action length too long. Only 895 characters taken..");
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Unknown format for message IRCPTACTION: " + args.Message + " \nEx: " + ex.Message);
                                return;
                            }
                        }
                        else if (Desc[5].Contains("|DELIVER|"))
                        {
                            try
                            {

                                rec.UserName = "MAILLOG_DELIVER";
                                string[] descText = args.Message.Split('|');
                                rec.EventCategory = "MESSAGE DELIVER";
                                rec.CustomStr5 = descText[descText.Length - 5].Split(':')[descText[descText.Length - 5].Split(':').Length - 1]; //Message ID
                                rec.CustomStr6 = descText[descText.Length - 4]; //Audit ID
                                rec.CustomStr2 = descText[descText.Length - 2]; // Mail Server IP Address
                                rec.CustomStr3 = descText[descText.Length - 1]; // Recipient Address

                                //dali
                                //rec.UserName = "MAILLOG_DELIVER";
                                //string[] descText = Desc[5].Split('|');
                                //rec.EventCategory = "MESSAGE DELIVER";
                                //rec.CustomStr5 = descText[0]; //Message ID
                                //rec.CustomStr6 = descText[1]; //Audit ID
                                //rec.CustomStr2 = descText[3]; // Mail Server IP Address
                                //rec.CustomStr3 = descText[4]; // Recipient Address

                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Unknown format for message DELIVER: " + args.Message + " \nEx: " + ex.Message);
                                return;
                            }

                        }
                        else if (Desc[5].Contains("|SENDER|"))
                        {
                            try
                            {
                                rec.UserName = "MAILLOG_SENDER";
                                string[] descText = Desc[5].Split('|');
                                rec.EventCategory = "MESSAGE SENDER";
                                rec.CustomStr5 = descText[0]; //Message ID
                                rec.CustomStr6 = descText[1]; //Audit ID
                                if (descText[3] == "\\")
                                {
                                    if (descText.Length > 4)
                                    {
                                        rec.CustomStr4 = descText[4]; // Sender Address
                                    }
                                    else
                                    {
                                        rec.CustomStr4 = "\\"; // Sender Address
                                    }
                                }
                                else
                                {
                                    rec.CustomStr4 = descText[3]; // Sender Address
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Unknown format for message SENDER: " + args.Message + " \nEx: " + ex.Message);
                                return;
                            }
                        }
                        else if (Desc[5].Contains("|ORCPTS|"))
                        {
                            try
                            {
                                rec.UserName = "MAILLOG_RECIPIENT";
                                string[] descText = Desc[5].Split('|');
                                rec.EventCategory = "MESSAGE ORCPTS";
                                rec.CustomStr5 = descText[0]; //Message ID
                                rec.CustomStr6 = descText[1]; //Audit ID
                                for (int i = 3; i < descText.Length; i++)
                                {
                                    rec.CustomStr3 += descText[i] + ",";
                                }
                                rec.CustomStr3 = rec.CustomStr3.TrimEnd(",".ToCharArray()); // Recipient Addresses

                                if (rec.CustomStr3.Length >= 6300)
                                {
                                    rec.CustomStr4 = rec.CustomStr3.Substring(900, 900);
                                    rec.CustomStr7 = rec.CustomStr3.Substring(1800, 900);
                                    rec.CustomStr8 = rec.CustomStr3.Substring(2700, 900);
                                    rec.CustomStr9 = rec.CustomStr3.Substring(3600, 900);
                                    rec.CustomStr10 = rec.CustomStr3.Substring(4500, 900);
                                    rec.CustomStr2 = rec.CustomStr3.Substring(5400, 900);
                                    rec.CustomStr3 = rec.CustomStr3.Substring(0, 900);
                                    L.Log(LogType.FILE, LogLevel.INFORM, "Recipient length longer than 6300. Only 6300 characters taken and data has been shared among other table fields..");
                                }
                                else if (rec.CustomStr3.Length >= 5400)
                                {
                                    rec.CustomStr4 = rec.CustomStr3.Substring(900, 900);
                                    rec.CustomStr7 = rec.CustomStr3.Substring(1800, 900);
                                    rec.CustomStr8 = rec.CustomStr3.Substring(2700, 900);
                                    rec.CustomStr9 = rec.CustomStr3.Substring(3600, 900);
                                    rec.CustomStr10 = rec.CustomStr3.Substring(4500, 900);
                                    rec.CustomStr2 = rec.CustomStr3.Substring(5400, rec.CustomStr3.Length - 5400);
                                    rec.CustomStr3 = rec.CustomStr3.Substring(0, 900);
                                    L.Log(LogType.FILE, LogLevel.INFORM, "Recipient length longer than 5400. Data has been shared among other table fields..");
                                }
                                else if (rec.CustomStr3.Length >= 4500)
                                {
                                    rec.CustomStr4 = rec.CustomStr3.Substring(900, 900);
                                    rec.CustomStr7 = rec.CustomStr3.Substring(1800, 900);
                                    rec.CustomStr8 = rec.CustomStr3.Substring(2700, 900);
                                    rec.CustomStr9 = rec.CustomStr3.Substring(3600, 900);
                                    rec.CustomStr10 = rec.CustomStr3.Substring(4500, rec.CustomStr3.Length - 4500);
                                    rec.CustomStr3 = rec.CustomStr3.Substring(0, 900);
                                    L.Log(LogType.FILE, LogLevel.INFORM, "Recipient length longer than 4500. Data has been shared among other table fields.");
                                }
                                else if (rec.CustomStr3.Length >= 3600)
                                {
                                    rec.CustomStr4 = rec.CustomStr3.Substring(900, 900);
                                    rec.CustomStr7 = rec.CustomStr3.Substring(1800, 900);
                                    rec.CustomStr8 = rec.CustomStr3.Substring(2700, 900);
                                    rec.CustomStr9 = rec.CustomStr3.Substring(3600, rec.CustomStr3.Length - 3600);
                                    rec.CustomStr3 = rec.CustomStr3.Substring(0, 900);
                                    L.Log(LogType.FILE, LogLevel.INFORM, "Recipient length longer than 3600. Data has been shared among other table fields.");
                                }
                                else if (rec.CustomStr3.Length >= 2700)
                                {
                                    rec.CustomStr4 = rec.CustomStr3.Substring(900, 900);
                                    rec.CustomStr7 = rec.CustomStr3.Substring(1800, 900);
                                    rec.CustomStr8 = rec.CustomStr3.Substring(2700, rec.CustomStr3.Length - 2700);
                                    rec.CustomStr3 = rec.CustomStr3.Substring(0, 900);
                                    L.Log(LogType.FILE, LogLevel.INFORM, "Recipient length longer than 2700. Data has been shared among other table fields.");
                                }
                                else if (rec.CustomStr3.Length >= 1800)
                                {
                                    rec.CustomStr4 = rec.CustomStr3.Substring(900, 900);
                                    rec.CustomStr7 = rec.CustomStr3.Substring(1800, rec.CustomStr3.Length - 1800);
                                    rec.CustomStr3 = rec.CustomStr3.Substring(0, 900);
                                    L.Log(LogType.FILE, LogLevel.INFORM, "Recipient length longer than 1800. Data has been shared among other table fields.");
                                }
                                else if (rec.CustomStr3.Length > 900)
                                {
                                    rec.CustomStr4 = rec.CustomStr3.Substring(900, rec.CustomStr3.Length - 900);
                                    rec.CustomStr3 = rec.CustomStr3.Substring(0, 900);
                                    L.Log(LogType.FILE, LogLevel.INFORM, "Recipient length longer than 900. Data has been shared among other table fields.");
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Unknown format for message ORCPTS: " + args.Message + " \nEx: " + ex.Message);
                                return;
                            }
                        }
                        else if (Desc[5].Contains("|ATTACH|"))
                        {
                            try
                            {
                                rec.UserName = "MAILLOG_ATTACH";
                                string[] descText = Desc[5].Split('|');
                                rec.EventCategory = "MESSAGE ATTACH";
                                rec.CustomStr5 = descText[0]; //Message ID
                                rec.CustomStr6 = descText[1]; //Audit ID
                                for (int i = 3; i < descText.Length; i++)
                                {
                                    rec.CustomStr3 += descText[i] + ",";
                                }
                                rec.CustomStr3 = rec.CustomStr3.TrimEnd(",".ToCharArray()); // Attached Documents
                                if (rec.CustomStr3.Length > 900)
                                {
                                    rec.CustomStr3 = rec.CustomStr3.Substring(0, 895) + "...";
                                    L.Log(LogType.FILE, LogLevel.INFORM, "Attachment length too long. Only 895 characters taken..");
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Unknown format for message ATTACH: " + args.Message + " \nEx: " + ex.Message);
                                return;
                            }
                        }
                        else
                        {
                            rec.UserName = "GENERAL";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Just put in description column. Ignored format: " + args.Message);
                        }
                        lastIndexForDesc = 5;
                    }
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Just put in description column. Very short message: " + args.Message);
                    lastIndexForDesc = 5;
                }


                for (int i = lastIndexForDesc; i < Desc.Length; i++)
                {
                    rec.Description += Desc[i] + ":";
                }
                rec.Description = rec.Description.TrimEnd(":".ToCharArray());
                if (rec.Description.Length > 900)
                {
                    rec.Description = rec.Description.Substring(0, 900);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");

                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
                if (usingRegistry)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(rec);
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetData(Dal, virtualhost, rec);
                    s.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");

            }
            catch (Exception er)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, er.ToString());
            }
        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\SymantecSmsSyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SymantecSmsSyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SymantecSmsSyslogRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SymantecSmsSyslogrecorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public bool Initialize_Logger()
        {
            try
            {
                L = new CLogger();
                switch (trc_level)
                {
                    case 0:
                        {
                            L.SetLogLevel(LogLevel.NONE);
                        } break;
                    case 1:
                        {
                            L.SetLogLevel(LogLevel.INFORM);
                        } break;
                    case 2:
                        {
                            L.SetLogLevel(LogLevel.WARN);
                        } break;
                    case 3:
                        {
                            L.SetLogLevel(LogLevel.ERROR);
                        } break;
                    case 4:
                        {
                            L.SetLogLevel(LogLevel.DEBUG);
                        } break;
                }

                L.SetLogFile(err_log);
                L.SetTimerInterval(LogType.FILE, logging_interval);
                L.SetLogFileSize(log_size);

                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SymantecSmsSyslog Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }


}
