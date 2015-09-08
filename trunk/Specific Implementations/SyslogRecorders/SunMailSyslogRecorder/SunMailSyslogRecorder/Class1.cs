//Onur Sarıkaya
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Parser;

namespace SunMailSyslogRecorder
{
    public class SunMailSyslogRecorder : CustomBase
    {

        public struct Fields
        {
            public string messagePi;
            public string LocalPi;
            public string line1;
            public string line2;
            public string line3;
            public string line4;
            public string fullLine;
            public bool RecordSequence;
        }

        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 4, Syslog_Port = 514, zone = 0;
        private string err_log, protocol = "UDP", location = "", remote_host = "localhost";
        private CLogger L;
        public Syslog slog = null;
        private bool reg_flag = false;
        protected bool usingRegistry = true;
        private ProtocolType pro;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;

        private Fields RecordFields;

        private void InitializeComponent()
        {
            RecordFields = new Fields();
        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
        String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
        String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
        String CustomVar1, int CustomVar2, String Virtualhost, String dal, Int32 Zone)
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
            remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            Dal = dal;
            zone = Zone;
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Syslog Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Syslog Recorder functions may not be running");
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
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(slog_SunMailSyslogRecorder);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Syslog Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\SunMailSyslogRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Syslog Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public SunMailSyslogRecorder()
        {
            /*
            try
            {
                // TODO: Add any initialization after the InitComponent call          
                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Recorder");

                L.Log(LogType.FILE, LogLevel.INFORM, "Start listening Syslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());

                slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port,System.Net.Sockets.ProtocolType.Tcp);
                slog.Start();
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(slog_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SyslogRecorder Constructor", er.ToString(), EventLogEntryType.Error);
            }
             */
        }

        // .net reflector'den önceki hali
        //public void slog_SunMailSyslogRecorder(LogMgrEventArgs args)
        //{
        //    CustomBase.Rec rec = new CustomBase.Rec();
        //    try
        //    {
        //        L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
        //        L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur  : " + args.Message);

        //        L.Log(LogType.FILE, LogLevel.DEBUG, "Line1,2,3,4, ataması öncesi --------------------------------------------- ");

        //        L.Log(LogType.FILE, LogLevel.DEBUG, "line1 : " + RecordFields.line1);
        //        L.Log(LogType.FILE, LogLevel.DEBUG, "line2 : " + RecordFields.line2);
        //        L.Log(LogType.FILE, LogLevel.DEBUG, "line3 : " + RecordFields.line3);
        //        L.Log(LogType.FILE, LogLevel.DEBUG, "line4 : " + RecordFields.line4);


        //        try
        //        {
        //            rec.LogName = "SunMailSyslogRecorder";
        //            rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        //            rec.EventType = args.EventLogEntType.ToString();


        //            // pi bilgisi alınıp recordfields'deki pi'ye atılıyor.
        //            string[] arr = args.Message.Split(' ');

        //            for (int i = 0; i < arr.Length; i++)
        //            {
        //                //L.Log(LogType.FILE, LogLevel.DEBUG, "arr[i] : " + arr[i]);

        //                if (arr[i].StartsWith("pi"))
        //                {
        //                    RecordFields.messagePi = arr[i].Split('=')[1].Replace('"', ' ');
        //                    L.Log(LogType.FILE, LogLevel.DEBUG, "pi Onur  : " + RecordFields.messagePi);
        //                }
        //                //1. satır bilgisi
        //                if (arr[i].StartsWith("fi="))
        //                {
        //                    RecordFields.line1 = args.Message;
        //                }
        //                //2. satır bilgisi
        //                if (arr[i].StartsWith("va="))
        //                {
        //                    if (arr[i].Contains("From:"))
        //                    {
        //                        RecordFields.line2 = args.Message;
        //                    }
        //                }
        //                //3. satır bilgisi
        //                if (arr[i].StartsWith("va="))
        //                {
        //                    if (arr[i].Contains("Subject:"))
        //                    {
        //                        RecordFields.line3 = args.Message;
        //                    }
        //                }
        //                //4. satır bilgisi
        //                if (arr[i].StartsWith("va="))
        //                {
        //                    if (arr[i].Contains("To:"))
        //                    {
        //                        RecordFields.line4 = args.Message;
        //                    }
        //                }
        //            }

        //            if (!string.IsNullOrEmpty(RecordFields.line1) && !string.IsNullOrEmpty(RecordFields.line2) && !string.IsNullOrEmpty(RecordFields.line3) && !string.IsNullOrEmpty(RecordFields.line4))
        //            {
        //                RecordFields.fullLine = RecordFields.line1 + " " + RecordFields.line2 + " " + RecordFields.line3 + " " + RecordFields.line4;

        //                L.Log(LogType.FILE, LogLevel.DEBUG, " 4 satır tamamlandı. ");
        //                L.Log(LogType.FILE, LogLevel.DEBUG, " fullLine : " + RecordFields.fullLine);

        //                try
        //                {
        //                    String[] fullQuotationMarksSplitted = RecordFields.fullLine.Split('"');

        //                    for (int i = 0; i < fullQuotationMarksSplitted.Length; i++)
        //                    {
        //                        try
        //                        {
        //                            if (fullQuotationMarksSplitted[i].StartsWith("To"))
        //                            {
        //                                string to = fullQuotationMarksSplitted[i].Split(':')[1];
        //                                if (fullQuotationMarksSplitted[i].Contains("@"))
        //                                {
        //                                    string[] toArray = fullQuotationMarksSplitted[i].Split(':')[1].Split(';');
        //                                    for (int j = 0; j < toArray.Length; j++)
        //                                    {
        //                                        if (toArray[j].Contains("@"))
        //                                        {
        //                                            L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr1: " + toArray[j]);
        //                                            rec.CustomStr1 += toArray[j].Replace("&quot", " ").Replace(" ", ";");
        //                                        }
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr1 - else : " + fullQuotationMarksSplitted[i].Split(':')[1]);
        //                                    rec.CustomStr1 = fullQuotationMarksSplitted[i].Split(':')[1];
        //                                }
        //                            }
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            L.Log(LogType.FILE, LogLevel.ERROR, " To  : " + ex.Message);
        //                        }

        //                        try
        //                        {
        //                            if (fullQuotationMarksSplitted[i].StartsWith("From"))
        //                            {
        //                                string from = fullQuotationMarksSplitted[i].Split(':')[1];
        //                                if (fullQuotationMarksSplitted[i].Contains("@"))
        //                                {
        //                                    string[] fromArray = fullQuotationMarksSplitted[i].Split(':')[1].Split(';');
        //                                    for (int j = 0; j < fromArray.Length; j++)
        //                                    {
        //                                        if (fromArray[j].Contains("@"))
        //                                        {
        //                                            rec.CustomStr3 += fromArray[j].Replace("&quot", " ").Replace(" ", ";");
        //                                        }
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    rec.CustomStr3 = fullQuotationMarksSplitted[i].Split(':')[1];
        //                                }
        //                            }
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            L.Log(LogType.FILE, LogLevel.ERROR, " From  : " + ex.Message);
        //                        }

        //                        //try
        //                        //{
        //                        //    if (rec.CustomStr3.Length == 0)
        //                        //    {
        //                        //        rec.CustomStr10 = RecordFields.line2;
        //                        //    }

        //                        //}
        //                        //catch (Exception ex)
        //                        //{
        //                        //    L.Log(LogType.FILE, LogLevel.ERROR, " Artık  : " + ex.Message);
        //                        //}

        //                        try
        //                        {
        //                            if (fullQuotationMarksSplitted[i].StartsWith("Subject"))
        //                            {
        //                                string subject = fullQuotationMarksSplitted[i].Split(':')[1];
        //                                rec.CustomStr2 = subject;
        //                            }
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            L.Log(LogType.FILE, LogLevel.ERROR, " Subject  : " + ex.Message);
        //                        }

        //                        try
        //                        {
        //                            if (fullQuotationMarksSplitted[i].StartsWith("dns"))
        //                            {
        //                                rec.CustomStr6 = (Between(fullQuotationMarksSplitted[i], "(", ")").Split('|')[1]);
        //                                rec.CustomStr7 = (Between(fullQuotationMarksSplitted[i], "(", ")").Split('|')[3]);
        //                            }
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            L.Log(LogType.FILE, LogLevel.ERROR, " DNS  : " + ex.Message);
        //                        }
        //                        //try
        //                        //{

        //                        //    if (rec.CustomStr1.Length > 899)
        //                        //    {
        //                        //        rec.CustomStr1 = rec.CustomStr1.Substring(0, 899);
        //                        //    }

        //                        //    if (rec.CustomStr2.Length > 899)
        //                        //    {
        //                        //        rec.CustomStr2 = rec.CustomStr2.Substring(0, 899);
        //                        //    }

        //                        //    if (rec.CustomStr3.Length > 899)
        //                        //    {
        //                        //        rec.CustomStr3 = rec.CustomStr3.Substring(0, 899);
        //                        //    }

        //                        //    if (rec.CustomStr10.Length > 899)
        //                        //    {
        //                        //        rec.CustomStr10 = rec.CustomStr10.Substring(0, 899);
        //                        //    }
        //                        //}
        //                        //catch (Exception ex)
        //                        //{
        //                        //    L.Log(LogType.FILE, LogLevel.ERROR, " Length Kontrol  : " + ex.Message);
        //                        //}
        //                    }
        //                    RecordFields.RecordSequence = true;
        //                }
        //                catch (Exception ex)
        //                {
        //                    L.Log(LogType.FILE, LogLevel.ERROR, "Split Error" + ex.Message);
        //                    L.Log(LogType.FILE, LogLevel.ERROR, "Split Error" + ex.StackTrace);
        //                }

        //                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1 : " + rec.CustomStr1);
        //                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2 : " + rec.CustomStr2);
        //                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3 : " + rec.CustomStr3);
        //                L.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName : " + rec.ComputerName);
        //            }

        //            else
        //            {
        //                //line'lar dolmadan insert yaptırma aksi halde from, to, subject boş olarak insert ediliyor.

        //            }
        //            L.Log(LogType.FILE, LogLevel.DEBUG, "Pi : " + RecordFields.messagePi);
        //            //                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1 : " + rec.CustomStr1);
        //            L.Log(LogType.FILE, LogLevel.DEBUG, "line1 : " + RecordFields.line1);
        //            L.Log(LogType.FILE, LogLevel.DEBUG, "line2 : " + RecordFields.line2);
        //            L.Log(LogType.FILE, LogLevel.DEBUG, "line3 : " + RecordFields.line3);
        //            L.Log(LogType.FILE, LogLevel.DEBUG, "line4 : " + RecordFields.line4);
        //            L.Log(LogType.FILE, LogLevel.DEBUG, "fullLine : " + RecordFields.fullLine);

        //            if (RecordFields.fullLine.Length > 4000)
        //            {
        //                rec.Description = RecordFields.fullLine.Substring(0, 3999);

        //            }
        //            else
        //            {
        //                rec.Description = RecordFields.fullLine;
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            L.Log(LogType.FILE, LogLevel.ERROR, "ERROR------------");
        //            L.Log(LogType.FILE, LogLevel.ERROR, e.Message);
        //            L.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
        //        }

        //        L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
        //        L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");

        //        if (usingRegistry)
        //        {
        //            CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
        //            if (RecordFields.RecordSequence)
        //            {
        //                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
        //                s.SetData(rec);
        //                ClearRecordFields();
        //                L.Log(LogType.FILE, LogLevel.DEBUG, "Finished sending Data and REcordFields cleared.");
        //            }
        //        }
        //        else
        //        {
        //            CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
        //            if (RecordFields.RecordSequence)
        //            {
        //                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
        //                s.SetData(Dal, virtualhost, rec);
        //                ClearRecordFields();
        //                L.Log(LogType.FILE, LogLevel.DEBUG, "Finished sending Data and REcordFields cleared.");
        //            }
        //            s.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);
        //        }
        //        L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
        //    }
        //    catch (Exception er)
        //    {
        //        L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
        //        L.Log(LogType.FILE, LogLevel.ERROR, args.EventLogEntType + " " + args.Message);
        //    }
        //}

        public void slog_SunMailSyslogRecorder(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {
                CustomServiceBase base2;
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur  : " + args.Message);

                L.Log(LogType.FILE, LogLevel.DEBUG, "Line1,2,3,4, ataması öncesi --------------------------------------------- ");

                L.Log(LogType.FILE, LogLevel.DEBUG, "line1 : " + RecordFields.line1);
                L.Log(LogType.FILE, LogLevel.DEBUG, "line2 : " + RecordFields.line2);
                L.Log(LogType.FILE, LogLevel.DEBUG, "line3 : " + RecordFields.line3);
                L.Log(LogType.FILE, LogLevel.DEBUG, "line4 : " + RecordFields.line4);
                try
                {
                    int num;
                    rec.LogName = "SunMailSyslogRecorder";
                    rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    rec.EventType = args.EventLogEntType.ToString();
                    string[] strArray = args.Message.Split(new char[] { ' ' });
                    for (num = 0; num < strArray.Length; num++)
                    {
                        if (strArray[num].StartsWith("pi"))
                        {
                             RecordFields.messagePi = strArray[num].Split(new char[] { '=' })[1].Replace('"', ' ');
                             L.Log(LogType.FILE, LogLevel.DEBUG, "pi Onur  : " +  RecordFields.messagePi, new int[0]);
                        }

                        if (strArray[num].StartsWith("fi="))
                        {
                             RecordFields.line1 = args.Message;
                        }
                        if (strArray[num].StartsWith("va=") && strArray[num].Contains("From:"))
                        {
                             RecordFields.line2 = args.Message;
                        }
                        if (strArray[num].StartsWith("va=") && strArray[num].Contains("Subject:"))
                        {
                             RecordFields.line3 = args.Message;
                        }
                        if (strArray[num].StartsWith("va=") && strArray[num].Contains("To:"))
                        {
                             RecordFields.line4 = args.Message;
                        }
                    }
                    if (((!string.IsNullOrEmpty( RecordFields.line1) && !string.IsNullOrEmpty( RecordFields.line2)) && !string.IsNullOrEmpty( RecordFields.line3)) && !string.IsNullOrEmpty( RecordFields.line4))
                    {
                        Exception exception;
                         RecordFields.fullLine =  RecordFields.line1 + " " +  RecordFields.line2 + " " +  RecordFields.line3 + " " +  RecordFields.line4;
                         L.Log(LogType.FILE, LogLevel.DEBUG, " 4 satır tamamlandı. ", new int[0]);
                         L.Log(LogType.FILE, LogLevel.DEBUG, " fullLine : " +  RecordFields.fullLine, new int[0]);
                        try
                        {
                            string[] strArray2 =  RecordFields.fullLine.Split(new char[] { '"' });
                            for (num = 0; num < strArray2.Length; num++)
                            {
                                int num2;
                                try
                                {
                                    if (strArray2[num].StartsWith("To"))
                                    {
                                        string str = strArray2[num].Split(new char[] { ':' })[1];
                                        if (strArray2[num].Contains("@"))
                                        {
                                            string[] strArray3 = strArray2[num].Split(new char[] { ':' })[1].Split(new char[] { ';' });
                                            num2 = 0;
                                            while (num2 < strArray3.Length)
                                            {
                                                if (strArray3[num2].Contains("@"))
                                                {
                                                    rec.CustomStr1 = rec.CustomStr1 + strArray3[num2].Replace("&quot", " ").Replace(" ", ";");
                                                }
                                                num2++;
                                            }
                                        }
                                        else
                                        {
                                            rec.CustomStr1 = strArray2[num].Split(new char[] { ':' })[1];
                                        }

                                        if (rec.CustomStr1.StartsWith(";"))
                                        {
                                            rec.CustomStr1 = rec.CustomStr1.Substring(1, rec.CustomStr1.Length - 1);
                                        }
                                    }
                                }
                                catch (Exception exception1)
                                {
                                    exception = exception1;
                                     L.Log(LogType.FILE, LogLevel.ERROR, " To  : " + exception.Message, new int[0]);
                                }
                                try
                                {
                                    if (strArray2[num].StartsWith("From"))
                                    {
                                        string str2 = strArray2[num].Split(new char[] { ':' })[1];
                                        if (strArray2[num].Contains("@"))
                                        {
                                            string[] strArray4 = strArray2[num].Split(new char[] { ':' })[1].Split(new char[] { ';' });
                                            for (num2 = 0; num2 < strArray4.Length; num2++)
                                            {
                                                if (strArray4[num2].Contains("@"))
                                                {
                                                    rec.CustomStr3 = rec.CustomStr3 + strArray4[num2].Replace("&quot", " ").Replace(" ", ";");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            rec.CustomStr3 = strArray2[num].Split(new char[] { ':' })[1];
                                        }

                                        if (rec.CustomStr3.StartsWith(";"))
                                        {
                                            rec.CustomStr3 = rec.CustomStr3.Substring(1, rec.CustomStr3.Length - 1);
                                        }
                                    }
                                }
                                catch (Exception exception4)
                                {
                                    exception = exception4;
                                     L.Log(LogType.FILE, LogLevel.ERROR, " From  : " + exception.Message, new int[0]);
                                }
                                try
                                {
                                    if (strArray2[num].StartsWith("Subject"))
                                    {
                                        string str3 = strArray2[num].Split(new char[] { ':' })[1];
                                        rec.CustomStr2 = str3;
                                    }
                                }
                                catch (Exception exception5)
                                {
                                    exception = exception5;
                                     L.Log(LogType.FILE, LogLevel.ERROR, " Subject  : " + exception.Message, new int[0]);
                                }
                                try
                                {
                                    if (strArray2[num].StartsWith("dns"))
                                    {
                                        rec.CustomStr6 = Between(strArray2[num], "(", ")").Split(new char[] { '|' })[1];
                                        rec.CustomStr7 = Between(strArray2[num], "(", ")").Split(new char[] { '|' })[3];
                                    }
                                }
                                catch (Exception exception6)
                                {
                                    exception = exception6;
                                     L.Log(LogType.FILE, LogLevel.ERROR, " DNS  : " + exception.Message, new int[0]);
                                }
                            }// for strarray2

                            string[] fg = args.Message.Split(' ');
                            for (int i = 0; i < fg.Length; i++)
                            {
                                try
                                {
                                    if (fg[i].StartsWith("no"))
                                    {
                                        rec.ComputerName = fg[i].Split('"')[1];
                                    }
                                }
                                catch (Exception exception7)
                                {
                                    exception = exception7;
                                     L.Log(LogType.FILE, LogLevel.ERROR, " Computer Name  : " + exception.Message, new int[0]);
                                }
                            }

                            
                             RecordFields.RecordSequence = true;
                        }
                        catch (Exception exception7)
                        {
                            exception = exception7;
                             L.Log(LogType.FILE, LogLevel.ERROR, "Split Error" + exception.Message, new int[0]);
                             L.Log(LogType.FILE, LogLevel.ERROR, "Split Error" + exception.StackTrace, new int[0]);
                        }
                         L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1 : " + rec.CustomStr1, new int[0]);
                         L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2 : " + rec.CustomStr2, new int[0]);
                         L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3 : " + rec.CustomStr3, new int[0]);
                         L.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName : " + rec.ComputerName, new int[0]);
                    }
                     L.Log(LogType.FILE, LogLevel.DEBUG, "Pi : " +  RecordFields.messagePi, new int[0]);
                     L.Log(LogType.FILE, LogLevel.DEBUG, "line1 : " +  RecordFields.line1, new int[0]);
                     L.Log(LogType.FILE, LogLevel.DEBUG, "line2 : " +  RecordFields.line2, new int[0]);
                     L.Log(LogType.FILE, LogLevel.DEBUG, "line3 : " +  RecordFields.line3, new int[0]);
                     L.Log(LogType.FILE, LogLevel.DEBUG, "line4 : " +  RecordFields.line4, new int[0]);
                     L.Log(LogType.FILE, LogLevel.DEBUG, "fullLine : " +  RecordFields.fullLine, new int[0]);
                }
                catch (Exception exception2)
                {
                     L.Log(LogType.FILE, LogLevel.ERROR, "ERROR------------", new int[0]);
                     L.Log(LogType.FILE, LogLevel.ERROR, exception2.Message, new int[0]);
                     L.Log(LogType.FILE, LogLevel.ERROR, exception2.StackTrace, new int[0]);
                }
                 L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record", new int[0]);
                 L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data", new int[0]);
                if ( usingRegistry)
                {
                    base2 = base.GetInstanceService("Security Manager Sender");
                    if ( RecordFields.RecordSequence)
                    {
                         L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data", new int[0]);
                        base2.SetData(rec);
                         ClearRecordFields();
                         L.Log(LogType.FILE, LogLevel.DEBUG, "Finished sending Data and REcordFields cleared.", new int[0]);
                    }
                }
                else
                {
                    base2 = base.GetInstanceService("Security Manager Remote Recorder");
                    if ( RecordFields.RecordSequence)
                    {
                         L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data", new int[0]);
                        base2.SetData( Dal,  virtualhost, rec);
                         ClearRecordFields();
                         L.Log(LogType.FILE, LogLevel.DEBUG, "Finished sending Data and REcordFields cleared.", new int[0]);
                    }
                    base2.SetReg( Id, rec.Datetime, "", "", "", rec.Datetime);
                }
                 L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data", new int[0]);
            }
            catch (Exception exception3)
            {
                 L.Log(LogType.FILE, LogLevel.ERROR, exception3.ToString(), new int[0]);
                 L.Log(LogType.FILE, LogLevel.ERROR, args.EventLogEntType + " " + args.Message, new int[0]);
            }
        }

        /// <summary>
        /// Bu fonksiyon bir string katarında belli olan iki karakter arasındaki stringi alabilmek için 
        /// yazılmıştır.
        /// </summary>
        /// <param name="value"></param> string değer
        /// <param name="a"></param> ilk string index
        /// <param name="b"></param> son string index
        /// <returns></returns>
        public static string Between(string value, string a, string b)
        {
            int posA = value.IndexOf(a);
            int posB = value.LastIndexOf(b);
            if (posA == -1)
            {
                return "";
            }
            if (posB == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= posB)
            {
                return "";
            }
            return value.Substring(adjustedPosA, posB - adjustedPosA);
        } // Between

        public void ClearRecordFields()
        {
            RecordFields.RecordSequence = false;
            RecordFields.line1 = "";
            RecordFields.line2 = "";
            RecordFields.line3 = "";
            RecordFields.line4 = "";
            RecordFields.fullLine = "";
            RecordFields.messagePi = "";

            L.Log(LogType.FILE, LogLevel.DEBUG, "RecordFields cleared.");

        } // ClearRecordFields 

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\CiscoBBSwitchSyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoBBSwitchSyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoBBSwitchSyslogRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Syslogrecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager Syslog Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
