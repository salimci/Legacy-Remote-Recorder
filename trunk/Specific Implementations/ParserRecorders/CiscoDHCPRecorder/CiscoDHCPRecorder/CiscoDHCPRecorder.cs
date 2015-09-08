//Name: Cisco DHCP Recorder
//Writer: Ali Yıldırım
//Date: 25.10.2010

using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using Parser;
using Log;
using CustomTools;
using SharpSSH.SharpSsh;
using System.Globalization;
using Microsoft.Win32;
using System.Diagnostics;
using System.Collections;

namespace CiscoDHCPRecorder
{
    public class CiscoDHCPRecorder : CustomBase
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
        // private SshExec se = null;

        public CiscoDHCPRecorder()
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on CiscoDHCPRecorder Recorder functions may not be running");
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
                EventLog.WriteEntry("Security Manager CiscoDHCPRecorder Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\CiscoDHCPRecorder" + ID + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoDHCPRecorder Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager McaffeeEpo Recorder Read Registry", er.ToString(), EventLogEntryType.Error);

                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        private StringReader GetData(SshShell se)
        {
            StringReader strR = null;
            string commandToRead = "show ip dhcp binding";
            Stream st = null;
            StreamReader sr = null;
            StringBuilder strB = new StringBuilder();
            bool morePage = false;

            while (true)
            {
                se.WriteLine(commandToRead);
                se.WriteLine(commandToRead);
                L.Log(LogType.FILE, LogLevel.DEBUG, " GetData() --> Komut çalıştırıldı : " + commandToRead);

                st = se.GetStream();
                sr = new StreamReader(st);
                
                int cnt = 0;
                int asci = 0;
                string lineStr = "";
                string previousStr = "";

                while ((asci = sr.Read()) != 0)
                {
                    char karakter = Convert.ToChar(asci);

                    if (karakter == '#')
                    {
                        if (cnt > 0)
                        {
                            break;
                        }
                        cnt++;
                    }

                    if (karakter != '\n')
                    {
                        lineStr += karakter.ToString();
                    }
                    else
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() | GetData() --> Read Line : " + lineStr);
                        if (lineStr.ToLower().Contains("more"))
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() | GetData() --> There is more ip to be read.");
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() | GetData() --> We will read more page.");
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() | GetData() --> Last ip line we have got : " + previousStr);
                            morePage = true;
                        }
                        else
                        {
                            previousStr = lineStr;
                            strB.Append(lineStr);
                        }
                        lineStr = "";
                    }
                }

                if (morePage)
                {
                    commandToRead = "show ip dhcp binding | begin " + previousStr;
                    morePage = false;
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() | GetData() --> All ip has got.");
                    break;
                }
            }
            
            strR = new StringReader(strB.ToString());

            if (se.Connected)
            {
                se.WriteLine("exit");
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() | GetData() --> Komut çalıştırıldı. exit");

                se.Close();
                L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() | GetData() --> Bağlantı kesildi.");
            }

            st.Close();
            sr.Close();

            return strR;
        }

        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Timer tetiklenme anı: " + e.SignalTime.ToLongTimeString());

            timer1.Enabled = false;
            String line = "";
            Rec rec = new Rec();
            recList = new ArrayList();
            SshShell se = null;
            StringReader strR = null;

            try
            {
                L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> SshShell nesnesi için. Host: " + remoteHost + ", User: " + user + ", Pass: *******");

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
                        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> Bağlantı açıldı.");
                    }
                    catch (Exception ex)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick() --> Connection problem. Hata : " + ex.ToString());
                    }
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> Zaten kurulu olan bağlantı ile devam ediliyor.");
                }

                if (se.Connected)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Bağlantı şuan açık.");
                    if (se.ShellOpened)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Shell şuan açık.");
                        if (se.ShellConnected)
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Shell Bağlantı kurdu.");
                            try
                            {
                                //We can collect data.
                                strR = GetData(se);
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick() --> Data Getirirken hata ile karşılaşıldı.");
                                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick() --> Hata: " + ex.ToString());
                            }
                        }
                    }
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Bağlantı kopuk!!!!");
                }

                while ((line = strR.ReadLine()) != null)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Line to Parse : " + line);
                    rec.Description = line;
                    String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (arr.Length >= 8)
                    {
                        rec.Description = line;

                        if (arr[0].Contains("."))
                        {
                            rec.ComputerName = remoteHost;
                            rec.CustomStr1 = arr[0];
                            tempMac = arr[1];

                            tempMac = tempMac.Remove(0, 2);
                            string[] arrMac = tempMac.Split('.');
                            tempMac = "";
                            for (int i = 0; i < arrMac.Length; i++)
                            {
                                tempMac += arrMac[i];
                            }
                            rec.CustomStr2 = tempMac.Trim();
                            rec.EventType = arr[7];

                            for (int i = 2; i < 6; i++)
                            {
                                tempDate += arr[i] + " ";
                            }
                            tempDate = tempDate.Trim();

                            if (arr[6] == "PM" && Convert.ToDateTime(tempDate).Hour != 12)
                            {
                                rec.Datetime = Convert.ToDateTime(tempDate, CultureInfo.InvariantCulture).AddDays(-8).AddHours(12).ToString("dd/MM/yyyy HH:mm:ss");
                            }
                            else
                            {
                                rec.Datetime = Convert.ToDateTime(tempDate, CultureInfo.InvariantCulture).AddDays(-8).ToString("dd/MM/yyyy HH:mm:ss");
                            }

                            if (Convert.ToDateTime(tempDate).Date.Day == DateTime.Now.Date.Day)
                            {
                                rec.CustomStr3 = "Removed";
                            }

                            tempDate = "";
                            recList.Add(rec);
                        }
                    }
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Çekilen veriler tarihlerine bakılarak kaydedilecek.");
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Kalınan LastPosition: " + Local_LastPosition);

                lastDate = new DateTime();
                bool lastPositionSetted = DateTime.TryParse(Local_LastPosition, out lastDate);

                foreach (Rec r in recList)
                {
                    if (!lastPositionSetted)
                    {
                        //İlk kayıtlar.
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetData(Dal, virtualhost, r);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> LastPosition is null. Veri ataması gerçekleştirildi.");
                    }
                    else
                    {
                        //Daha önce kayıt alınmış.
                        if (Convert.ToDateTime(r.Datetime) > lastDate)
                        {
                            //Değişen kayıt olmuş. Veritabanına eklenmeli.
                            CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                            s.SetData(Dal, virtualhost, r);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Record Date daha büyük. Veri ataması gerçekleştirildi. : " + r.Datetime);
                        }
                        else
                        {
                            //Tarih aynı. Kayıt işlemi yapılmayacak.
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Record Date daha küçük. Kaydedilmedi!. Tarih: " + r.Datetime);
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
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Yeni LastPosition atandı. LastPostion: " + Local_LastPosition);

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
                        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> Bağlantı kesildi.");
                    }
                }

                se = null;
                tempDate = "";
                tempMac = "";
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Timer'ın işi bitti. Time: " + DateTime.Now.ToLongTimeString());
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Bir Sonraki Tetikleme " + timer_interval / 60000 + " dakika sonra gerçekleşecektir.");
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> =============================================== ");
                recList.Clear();
            }
        }

        private void timer1_Tick_old(object sender, System.Timers.ElapsedEventArgs e)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Timer tetiklenme anı: " + e.SignalTime.ToLongTimeString());

            timer1.Enabled = false;
            String stdOut = "";
            String stdErr = "";
            String line = "";
            Rec rec = new Rec();
            recList = new ArrayList();
            SshExec se = null;

            try
            {
                L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> SshExec nesnesi için. Host: " + remoteHost + ", User: " + user + ", Pass: *******");

                se = new SshExec(remoteHost, user);
                if (!String.IsNullOrEmpty(password))
                    se.Password = password;

                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> SshExec nesnesi üretildi.");

                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Started to connect and parse lines.");

                if (!se.Connected)
                {
                    try
                    {
                        se.Connect(22, 2000);
                        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> Bağlantı açıldı.");
                        se.SetTimeout(15000);
                        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> Timeout atandı. 15000");
                    }
                    catch (Exception ex)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick() --> Connection problem. Hata : " + ex.ToString());
                    }
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> Zaten kurulu olan bağlantı ile devam ediliyor.");
                }

                if (se.Connected)
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> Bağlantı şuan açık. Komut çalıştırılabilir.");
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> Bağlantı şuan kapalı. Komut çalışmayacak!!");
                }

                try
                {
                    se.RunCommand("show ip dhcp binding ", ref stdOut, ref stdErr);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Komut çalıştırıldı. show ip dhcp binding");
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick() --> Komut çalıştırılamadı!!!   show ip dhcp binding");
                    L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick() --> Hata: " + ex.ToString());
                }

                String stdOut1 = "";
                String stdErr1 = "";

                if (se.Connected)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Bağlantı şuan açık. Komut çalıştırılabilir.");
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Bağlantı kopuk. Komut çalışmayacak!!");
                }

                try
                {
                    se.RunCommand("exit", ref stdOut1, ref stdErr1);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Komut çalıştırıldı. exit");
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick() --> Komut çalıştırılamadı!!!  exit");
                    L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick() --> Hata: " + ex.ToString());
                }

                if (se.Connected)
                {
                    se.Close();
                    L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> Bağlantı kesildi.");
                }

                StringReader sr = new StringReader(stdOut);

                while ((line = sr.ReadLine()) != null)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Line: " + line);
                    rec.Description = line;
                    String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (arr.Length >= 8)
                    {
                        rec.Description = line;

                        if (arr[0].Contains("."))
                        {
                            rec.ComputerName = remoteHost;
                            rec.CustomStr1 = arr[0];
                            tempMac = arr[1];

                            tempMac = tempMac.Remove(0, 2);
                            string[] arrMac = tempMac.Split('.');
                            tempMac = "";
                            for (int i = 0; i < arrMac.Length; i++)
                            {
                                tempMac += arrMac[i];
                            }
                            rec.CustomStr2 = tempMac.Trim();
                            rec.EventType = arr[7];

                            for (int i = 2; i < 6; i++)
                            {
                                tempDate += arr[i] + " ";
                            }
                            tempDate = tempDate.Trim();

                            if (arr[6] == "PM" && Convert.ToDateTime(tempDate).Hour != 12)
                            {
                                rec.Datetime = Convert.ToDateTime(tempDate, CultureInfo.InvariantCulture).AddDays(-8).AddHours(12).ToString("dd/MM/yyyy HH:mm:ss");
                            }
                            else
                            {
                                rec.Datetime = Convert.ToDateTime(tempDate, CultureInfo.InvariantCulture).AddDays(-8).ToString("dd/MM/yyyy HH:mm:ss");
                            }

                            if (Convert.ToDateTime(tempDate).Date.Day == DateTime.Now.Date.Day)
                            {
                                rec.CustomStr3 = "Removed";
                            }

                            tempDate = "";
                            recList.Add(rec);
                        }
                    }
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Çekilen veriler tarihlerine bakılarak kaydedilecek.");
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Kalınan LastPosition: " + Local_LastPosition);

                lastDate = new DateTime();
                bool lastPositionSetted = DateTime.TryParse(Local_LastPosition, out lastDate);

                foreach (Rec r in recList)
                {
                    if (!lastPositionSetted)
                    {
                        //İlk kayıtlar.
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetData(Dal, virtualhost, r);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> LastPosition is null. Veri ataması gerçekleştirildi.");
                    }
                    else
                    {
                        //Daha önce kayıt alınmış.
                        if (Convert.ToDateTime(r.Datetime) > lastDate)
                        {
                            //Değişen kayıt olmuş. Veritabanına eklenmeli.
                            CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                            s.SetData(Dal, virtualhost, r);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Record Date daha büyük. Veri ataması gerçekleştirildi. : " + r.Datetime);
                        }
                        else
                        {
                            //Tarih aynı. Kayıt işlemi yapılmayacak.
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Record Date daha küçük. Kaydedilmedi!. Tarih: " + r.Datetime);
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
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Yeni LastPosition atandı. LastPostion: " + Local_LastPosition);
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
                        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() --> Bağlantı kesildi.");
                    }
                }

                tempDate = "";
                tempMac = "";
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Timer'ın işi bitti. Time: " + DateTime.Now.ToLongTimeString());
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> Bir Sonraki Tetikleme " + timer_interval / 60000 + " dakika sonra gerçekleşecektir.");
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() --> ===============================================");
                recList.Clear();
            }
        }

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


