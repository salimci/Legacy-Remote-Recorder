//Name: Cisco DHCP Recorder
//TCDD 3.Bölge
//Writer: Onur Sarikaya
//Date: 10.01.2013

using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using NatekInfra.Connection;
using Parser;
using Log;
using CustomTools;
using SharpSSH.SharpSsh;
using System.Globalization;
using Microsoft.Win32;
using System.Diagnostics;
using System.Collections;

namespace CiscoDhcpV_1_0_0Recorder
{
    public class CiscoDhcpV_1_0_0Recorder : CustomBase
    {
        private System.Timers.Timer timer1;
        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 4, zone = 0;
        private string err_log;
        private int ID;
        private CLogger L;
        private bool reg_flag = false;
        private int timer_interval = 1800000;
        private string user = "";
        private string password = "";
        private string remoteHost = "localhost";
        private bool usingRegistry = true;
        private string virtualhost, Dal;
        private string location;
        private string maxDate = "";
        private string lastPosition = "";
        private string tempDate = "";
        private string tempMac = "";
        private DateTime lastDate;
        private string Local_LastPosition = "";
        ArrayList recList;
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";
        // private SshExec se = null;

        public CiscoDhcpV_1_0_0Recorder()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {

        }

        public override void Init()
        {
            timer1 = new System.Timers.Timer();
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);
            timer1.Interval = timer_interval;
            timer1.Enabled = true;

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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on CiscoDhcpV_1_0_0Recorder functions may not be running");
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
                            //L.Log(LogType.FILE, LogLevel.ERROR, " Error on Reading the Registry ");
                            return;
                        }
                        else
                            if (!Initialize_Logger())
                            {
                                //L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Cisco DHCP Recorder functions may not be running");
                                return;
                            }
                        reg_flag = true;
                    }
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoDhcpV_1_0_0Recorder Init", er.ToString(), EventLogEntryType.Error);
            }


        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\CiscoDhcpV_1_0_0Recorder" + ID + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoDhcpV_1_0_0Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
        String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
        String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
        String CustomVar1, int CustomVar2, String Virtualhost, String dal, Int32 Zone)
        {
            trc_level = TraceLevel;
            remoteHost = RemoteHost;//Host
            user = User;//User
            password = Password;//Password
            location = Location;
            Dal = dal;
            usingRegistry = false;
            zone = Zone;
            virtualhost = Virtualhost;
            lastPosition = LastPosition;
            Local_LastPosition = LastPosition;//Last position as date
            ID = Identity;
            if (SleepTime != 0)
            {
                timer_interval = SleepTime;//Timer interval
            }
        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoDhcpV_1_0_0Recorder Read Registry", er.ToString(), EventLogEntryType.Error);

                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        private StringReader GetData(string host, string user, string password, SshShell se)
        {
            StringReader strR = null;
            int asc = 012;
            string commandToRead = "show ip arp";
            //string defaultCommand = "terminal length 0";
            Stream st = null;
            StreamReader sr = null;
            StringBuilder strB = new StringBuilder();

            Stream st1 = null;
            StreamReader sr1 = null;
            StringBuilder strB1 = new StringBuilder();

            bool morePage = false;
            IConnector sshConn = ConnectionManager.getConnector("SSH");
            sshConn.SetConfigData(L);
            sshConn.Init();
            string stdOut;
            StringReader reader = null;

            if (!sshConn.initConnection(host, user, password, password, 0))
            {
                //log failed to conect
                return new StringReader("");
            }

            string defaultCommand = "terminal length 0";
            sshConn.write(defaultCommand);
            L.Log(LogType.FILE, LogLevel.DEBUG, " GetData() --> Default Komut calistirildi : " + defaultCommand);

            string result = sshConn.read(2000);
            result = result.Replace('\0', ' ');
            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() | GetData() --> Read Line Default : " + result);

            while (true)
            {
                sshConn.write(commandToRead);
                L.Log(LogType.FILE, LogLevel.DEBUG, " GetData() --> Komut calistirildi : " + commandToRead);
                stdOut = sshConn.read(10000);
                L.Log(LogType.FILE, LogLevel.DEBUG, " GetData() --> Komut cevabı : " + stdOut);

                int cnt = 0;
                int asci = 0;
                string lineStr = "";
                string previousStr = "";
                reader = new StringReader(stdOut);

                //while ((asci = reader.Read()) != 0)
                //{
                //    char karakter = Convert.ToChar(asci);//

                //    if (karakter == '#')
                //    {
                //        if (cnt > 0)
                //        {
                //            break;
                //        }
                //        cnt++;
                //    }

                //    //if (karakter != '\n')
                //    if (karakter != '\n')
                //    {
                //        lineStr += karakter.ToString();
                //    }
                //    else
                //    {
                //        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() | GetData() --> Read Line : " + lineStr);
                //        if (lineStr.ToLower().Contains("more"))
                //        {
                //            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() | GetData() --> There is more ip to be read.");
                //            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() | GetData() --> We will read more page.");
                //            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() | GetData() --> Last ip line we have got : " + previousStr);
                //            morePage = true;
                //        }
                //        else
                //        {
                //            previousStr = lineStr;
                //            strB.Append(lineStr);
                //        }
                //        lineStr = "";
                //    }
                //}

                L.Log(LogType.FILE, LogLevel.DEBUG, " GetData() --> End of While. ");

                if (morePage)
                {
                    int s = 32;
                    commandToRead = "show ip arp | ";
                    //commandToRead = "/";
                    //commandToRead = " ";//
                    morePage = false;
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() | GetData() --> All ip has got.");
                    break;
                }


            }

            L.Log(LogType.FILE, LogLevel.DEBUG, " GetData() --> End of While true. ");

            //strR = new StringReader(strB.ToString());
            //strR = new StringReader(reader);

            /*if (se.Connected)
            {
                se.WriteLine("exit");
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() | GetData() --> Komut calistirildi. exit");

                se.Close();
                L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() | GetData() --> Baglanti kesildi.");
            }*/

            //st.Close();
            //sr.Close();

            sshConn.dropConnection();

            return reader;
        }

        /// <summary>
        /// line space split function
        /// </summary>
        /// <param name="line"></param>
        /// gelen line 
        /// <param name="useTabs"></param>
        /// eğer line içinde tab boşluk var ise ve buna göre de split yapılmak isteniyorsa true
        /// eğer line içinde tab boşluk var ise ve buna göre  split yapılmak istenmiyorsa false
        /// <returns></returns>
        public virtual String[] SpaceSplit(String line, bool useTabs)
        {
            List<String> lst = new List<String>();
            StringBuilder sb = new StringBuilder();
            bool space = false;
            foreach (Char c in line.ToCharArray())
            {
                if (c != ' ' && (!useTabs || c != '\t'))
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
        }// SpaceSplit

        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Timer tetiklenme ani: " + e.SignalTime.ToLongTimeString());

            timer1.Enabled = false;
            String line = "";
            Rec rec = new Rec();
            recList = new ArrayList();
            SshShell se = null;
            StringReader strR = null;

            try
            {
                L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> SshShell nesnesi icin. Host: " + remoteHost + ", User: " + user + ", Pass: *******");

                se = new SshShell(remoteHost, user);
                if (!String.IsNullOrEmpty(password))
                    se.Password = password;

                //L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Started to connect and parse lines.");
                if (se != null)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> SshShell nesnesi Üretildi.");
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> SshShell nesnesi Üretilemedi.");
                }

                if (!se.Connected)
                {
                    try
                    {
                        se.Connect(22, 2000);
                        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> Baglanti acildi.");
                    }
                    catch (Exception ex)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick() --> Connection problem. Hata : " + ex.ToString());
                    }
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> Zaten kurulu olan Baglanti ile devam ediliyor.");
                }

                if (se.Connected)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Baglanti suan acik.");
                    if (se.ShellOpened)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Shell suan acik.");
                        if (se.ShellConnected)
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Shell Baglanti kurdu.");
                            try
                            {
                                //We can collect data.
                                strR = GetData(remoteHost, user, password, se);

                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick() --> Data Getirirken hata ile karsilasildi.");
                                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick() --> Hata: " + ex.ToString());
                            }
                        }
                    }
                    
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Baglanti kopuk!!!!");
                }

                while ((line = strR.ReadLine()) != null)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Line to Parse : " + line);
                    rec.Description = line;
                    //String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    String[] arr = SpaceSplit(line, false);
                    if (line != "show ip arp")
                    {
                        try
                        {
                            rec.Description = line;
                            rec.CustomStr3 = arr[0];
                            rec.CustomStr1 = arr[1];
                            try
                            {
                                rec.CustomInt1 = Convert.ToInt32(arr[2]);
                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> CustomInt1 ERROR: " + exception);
                                rec.CustomInt1 = 0;
                            }

                            rec.CustomStr2 = arr[3];
                            rec.EventType = arr[4];
                            rec.EventCategory = arr[5];
                            rec.Datetime = DateTime.Now.ToString(dateFormat);

                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Datetime : " + rec.Datetime);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Customstr1 : " + rec.CustomStr1);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Customstr3 : " + rec.CustomStr3);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Customstr2 : " + rec.CustomStr2);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> EventCategory : " + rec.EventCategory);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> EventType : " + rec.EventType);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> CustomInt1 : " + rec.CustomInt1);
                            recList.Add(rec);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> ERROR: " + exception.Message);
                        }
                    }
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> cekilen veriler tarihlerine bakilarak kaydedilecek.");
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Kalinan LastPosition: " + Local_LastPosition);

                lastDate = new DateTime();
                bool lastPositionSetted = DateTime.TryParse(Local_LastPosition, out lastDate);
                rec.LogName = "CiscoDhcpV_1_0_0Recorder";
                foreach (Rec r in recList)
                {
                    if (!lastPositionSetted)
                    {
                        //İlk kayitlar.
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetData(Dal, virtualhost, r);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> LastPosition is null. Veri atamasi gerceklestirildi.");
                    }
                    else
                    {
                        //Daha önce kayit alinmis.
                        if (Convert.ToDateTime(r.Datetime) > lastDate)
                        {
                            //Değisen kayit olmus. Veritabanina eklenmeli.
                            CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                            s.SetData(Dal, virtualhost, r);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Record Date daha büyük. Veri atamasi gerceklestirildi. : " + r.Datetime);
                        }
                        else
                        {
                            //Tarih ayni. Kayit islemi yapilmayacak.
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Record Date daha kücük. Kaydedilmedi!. Tarih: " + r.Datetime);
                        }
                    }
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> En büyük tarih belirleniyor.");
                maxDate = lastDate.ToString();

                foreach (Rec r in recList)
                {
                    if (Convert.ToDateTime(r.Datetime) > Convert.ToDateTime(maxDate))
                    {
                        maxDate = r.Datetime;
                    }
                }

                CustomServiceBase ser = base.GetInstanceService("Security Manager Remote Recorder");
                ser.SetReg(ID, maxDate, "", "", "");
                Local_LastPosition = maxDate;
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Yeni LastPosition atandi. LastPostion: " + Local_LastPosition);

            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick() --> Recorder Main Exception: " + ex.ToString());
            }
            finally
            {
                if (se != null)
                {
                    if (se.Connected)
                    {
                        se.Close();
                        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> Baglanti kesildi.");
                    }
                }

                se = null;
                tempDate = "";
                tempMac = "";
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Timer'in isi bitti. Time: " + DateTime.Now.ToLongTimeString());
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Bir Sonraki Tetikleme " + timer_interval / 60000 + " dakika sonra gerceklesecektir.");
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> =============================================== ");
                recList.Clear();
                
            }
            se.Close();
        }

        //private void timer1_Tick_old(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Timer tetiklenme ani: " + e.SignalTime.ToLongTimeString());

        //    timer1.Enabled = false;
        //    String stdOut = "";
        //    String stdErr = "";
        //    String line = "";
        //    Rec rec = new Rec();
        //    recList = new ArrayList();
        //    SshExec se = null;

        //    try
        //    {
        //        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> SshExec nesnesi icin. Host: " + remoteHost + ", User: " + user + ", Pass: *******");

        //        se = new SshExec(remoteHost, user);
        //        if (!String.IsNullOrEmpty(password))
        //            se.Password = password;

        //        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> SshExec nesnesi üretildi.");

        //        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Started to connect and parse lines.");

        //        if (!se.Connected)
        //        {
        //            try
        //            {
        //                se.Connect(22, 2000);
        //                L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> Baglanti acildi.");
        //                se.SetTimeout(15000);
        //                L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> Timeout atandi. 15000");
        //            }
        //            catch (Exception ex)
        //            {
        //                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick() --> Connection problem. Hata : " + ex.ToString());
        //            }
        //        }
        //        else
        //        {
        //            L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> Zaten kurulu olan Baglanti ile devam ediliyor.");
        //        }

        //        if (se.Connected)
        //        {
        //            L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> Baglanti suan acik. Komut calistirilabilir.");
        //        }
        //        else
        //        {
        //            L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> Baglanti suan kapali. Komut calismayacak!!");
        //        }

        //        try
        //        {
        //            se.RunCommand("show ip dhcp binding ", ref stdOut, ref stdErr);
        //            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Komut calistirildi. show ip dhcp binding");
        //        }
        //        catch (Exception ex)
        //        {
        //            L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick() --> Komut calistirilamadi!!!   show ip dhcp binding");
        //            L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick() --> Hata: " + ex.ToString());
        //        }

        //        String stdOut1 = "";
        //        String stdErr1 = "";

        //        if (se.Connected)
        //        {
        //            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Baglanti suan acik. Komut calistirilabilir.");
        //        }
        //        else
        //        {
        //            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Baglanti kopuk. Komut calismayacak!!");
        //        }

        //        try
        //        {
        //            se.RunCommand("exit", ref stdOut1, ref stdErr1);
        //            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Komut calistirildi. exit");
        //        }
        //        catch (Exception ex)
        //        {
        //            L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick() --> Komut calistirilamadi!!!  exit");
        //            L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick() --> Hata: " + ex.ToString());
        //        }

        //        if (se.Connected)
        //        {
        //            se.Close();
        //            L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> Baglanti kesildi.");
        //        }

        //        StringReader sr = new StringReader(stdOut);

        //        while ((line = sr.ReadLine()) != null)
        //        {
        //            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Line: " + line);
        //            rec.Description = line;
        //            String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        //            if (arr.Length >= 8)
        //            {
        //                rec.Description = line;

        //                if (arr[0].Contains("."))
        //                {
        //                    rec.ComputerName = remoteHost;
        //                    rec.CustomStr1 = arr[0];
        //                    tempMac = arr[1];

        //                    tempMac = tempMac.Remove(0, 2);
        //                    string[] arrMac = tempMac.Split('.');
        //                    tempMac = "";
        //                    for (int i = 0; i < arrMac.Length; i++)
        //                    {
        //                        tempMac += arrMac[i];
        //                    }
        //                    rec.CustomStr2 = tempMac.Trim();
        //                    rec.EventType = arr[7];

        //                    for (int i = 2; i < 6; i++)
        //                    {
        //                        tempDate += arr[i] + " ";
        //                    }
        //                    tempDate = tempDate.Trim();

        //                    if (arr[6] == "PM" && Convert.ToDateTime(tempDate).Hour != 12)
        //                    {
        //                        rec.Datetime = Convert.ToDateTime(tempDate, CultureInfo.InvariantCulture).AddDays(-8).AddHours(12).ToString("dd/MM/yyyy HH:mm:ss");
        //                    }
        //                    else
        //                    {
        //                        rec.Datetime = Convert.ToDateTime(tempDate, CultureInfo.InvariantCulture).AddDays(-8).ToString("dd/MM/yyyy HH:mm:ss");
        //                    }

        //                    if (Convert.ToDateTime(tempDate).Date.Day == DateTime.Now.Date.Day)
        //                    {
        //                        rec.CustomStr3 = "Removed";
        //                    }

        //                    tempDate = "";
        //                    recList.Add(rec);
        //                }
        //            }
        //        }

        //        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> cekilen veriler tarihlerine bakilarak kaydedilecek.");
        //        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Kalinan LastPosition: " + Local_LastPosition);

        //        lastDate = new DateTime();
        //        bool lastPositionSetted = DateTime.TryParse(Local_LastPosition, out lastDate);

        //        foreach (Rec r in recList)
        //        {
        //            if (!lastPositionSetted)
        //            {
        //                //İlk kayitlar.
        //                CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
        //                s.SetData(Dal, virtualhost, r);
        //                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> LastPosition is null. Veri atamasi gerceklestirildi.");
        //            }
        //            else
        //            {
        //                //Daha önce kayit alinmis.
        //                if (Convert.ToDateTime(r.Datetime) > lastDate)
        //                {
        //                    //Değisen kayit olmus. Veritabanina eklenmeli.
        //                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
        //                    s.SetData(Dal, virtualhost, r);
        //                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Record Date daha büyük. Veri atamasi gerceklestirildi. : " + r.Datetime);
        //                }
        //                else
        //                {
        //                    //Tarih ayni. Kayit islemi yapilmayacak.
        //                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Record Date daha kücük. Kaydedilmedi!. Tarih: " + r.Datetime);
        //                }
        //            }
        //        }

        //        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> En büyük tarih belirleniyor.");
        //        maxDate = lastDate.ToString();

        //        foreach (Rec r in recList)
        //        {
        //            if (Convert.ToDateTime(r.Datetime) > Convert.ToDateTime(maxDate))
        //            {
        //                maxDate = r.Datetime;
        //            }
        //        }

        //        CustomServiceBase ser = base.GetInstanceService("Security Manager Remote Recorder");
        //        ser.SetReg(ID, maxDate, "", "", "");
        //        Local_LastPosition = maxDate;
        //        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Yeni LastPosition atandi. LastPostion: " + Local_LastPosition);
        //    }
        //    catch (Exception ex)
        //    {
        //        L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick() --> Recorder Main Exception: " + ex.ToString());
        //    }
        //    finally
        //    {
        //        if (se != null)
        //        {
        //            if (se.Connected)
        //            {
        //                se.Close();
        //                L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> Baglanti kesildi.");
        //            }
        //        }

        //        tempDate = "";
        //        tempMac = "";
        //        timer1.Enabled = true;
        //        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Timer'in isi bitti. Time: " + DateTime.Now.ToLongTimeString());
        //        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Bir Sonraki Tetikleme " + timer_interval / 60000 + " dakika sonra gerceklesecektir.");
        //        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> ===============================================");
        //        recList.Clear();
        //    }
        //}

        public override void Clear()
        {
            if (timer1 != null)
                timer1.Enabled = false;
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
                EventLog.WriteEntry("Security Manager CiscoDHCP Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        public bool Set_Registry(long status)
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("McaffeeEpoRecorder");
                rk.SetValue("LastRecordNum", status);
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager CiscoDHCP Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }
    }
}


