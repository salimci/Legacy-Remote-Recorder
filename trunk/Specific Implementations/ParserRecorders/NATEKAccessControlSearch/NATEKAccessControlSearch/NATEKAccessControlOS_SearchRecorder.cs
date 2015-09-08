using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Log;
using System.Collections;
using System.IO;
using CustomTools;
using System.Timers;
using Microsoft.Win32;

namespace Parser
{
  public class NATEKAccessControlOS_SearchRecorder : Parser
  {

    #region MEMBERS
    string eventCategory;
    string eventType;
    string datetime;
    #endregion

    public NATEKAccessControlOS_SearchRecorder()
      : base()
    {
      LogName = "NATEKAccessControlOS_SearchRecorder";//
      eventType = "Allow";
      eventCategory = "NatekSearch";
      //gfh
      enc = Encoding.UTF8;
    } // NATEKAccessControlOS_SearchRecorder

    public override void Init()
    {
      GetFiles();
    } // Init

    public NATEKAccessControlOS_SearchRecorder(String fileName)
      : base(fileName)
    {
      LogName = "NATEKAccessControlOS_SearchRecorder";
      // fdsfdsfdsf
      eventType = "Allow";
      eventCategory = "NatekSearch";
    }// NATEKAccessControlOS_SearchRecorder

    protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
      dayChangeTimer.Enabled = false;
      if (remoteHost == "")
      {
        String fileLast = FileName;
        ParseFileName();
        if (FileName != fileLast)
        {
          Stop();
          Position = 0;
          lastLine = "";
          lastFile = FileName;
          Start();
          Log.Log(LogType.FILE, LogLevel.DEBUG, "dayChangeTimer_Elapsed() -->> File Changed, New File Is, " + FileName);
        }
        else
        {
          FileInfo fi = new FileInfo(FileName);
          if (fi.Length - 1 > Position)
          {
            Stop();
            Start();
          }
          Log.Log(LogType.FILE, LogLevel.DEBUG, "dayChangeTimer_Elapsed() -->> Day Change Timer File Is: " + FileName);
        }

        dayChangeTimer.Enabled = true;
      }
    } // dayChangeTimer_Elapsed

    public override void SetConfigData(int Identity, string Location, string LastLine, string LastPosition, string LastFile, string LastKeywords, bool FromEndOnLoss, int MaxLineToWait, string User, string Password, string RemoteHost, int SleepTime, int TraceLevel, string CustomVar1, int CustomVar2, string virtualhost, string dal, int Zone)
    {
      base.SetConfigData(Identity, Location, LastLine, LastPosition, LastFile, LastKeywords, FromEndOnLoss, MaxLineToWait, User, Password, RemoteHost, SleepTime, TraceLevel, CustomVar1, CustomVar2, virtualhost, dal, Zone);

      FileName = LastFile;

      try
      {
        string[] arr = FileName.Split('-');
        //eventCategory = arr[1];
        datetime = resolveDatetime(arr[2], arr[3]);
      }
      catch (Exception e)
      {
        Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameLocal() --> unrecognized file name!.");
        DateTime dt = File.GetCreationTime(FileName);
        datetime = dt.ToString("{yyyy-MM-dd HH:mm:ss}");
      }
    } // SetConfigData

    public override String GetLocation()
    {
      Log.Log(LogType.FILE, LogLevel.DEBUG, "  NATEKAccessControlOSRecorder In GetLocation() -->> Enter The Function");

      string templocationregistry = "";
      string templocationspecialfolder = "";
      string templocation = "";

      if (usingRegistry)
      {
        try
        {
          if (reg == null)
          {
            reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\NATEK\\Security Manager\\Recorder\\" + LogName, true);
            //reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\NATEK\\Security Manager\\Recorder\\AccessControlRecorder", true);
          }
          templocationregistry = reg.GetValue("Location").ToString();
          templocationregistry = templocationregistry + "\\";
          templocationspecialfolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
          templocation = templocationspecialfolder + "\\" + templocationregistry;
        }
        catch (Exception ex)
        {
          Log.Log(LogType.FILE, LogLevel.ERROR, "  Catch In GetLocation() : " + ex.Message + " templocation  is : " + templocation);
        }
        Log.Log(LogType.FILE, LogLevel.DEBUG, "  NATEKAccessControlOSRecorder In GetLocation() -->> Location is : " + templocation);
        Log.Log(LogType.FILE, LogLevel.DEBUG, "  NATEKAccessControlOSRecorder In GetLocation() -->> Exit The Function");
        return templocation;
      }
      else
      {
        Log.Log(LogType.FILE, LogLevel.DEBUG, "  NATEKAccessControlOSRecorder In GetLocation() -->> Location is : " + Dir);
        Log.Log(LogType.FILE, LogLevel.DEBUG, "  NATEKAccessControlOSRecorder In GetLocation() -->> Exit The Function");
        return Dir;
      }
    } // GetLocation

    public override bool ParseSpecific(String line, bool dontSend)
    {
      Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");

      if (line == "")
        return true;

      if (!dontSend)
      {
        String[] arr;

        arr = line.Split('\\');

        try
        {
          Rec r = new Rec();

          r.ComputerName = Environment.MachineName;
          r.EventType = eventType;
          r.EventCategory = eventCategory;
          r.Datetime = datetime;
          datetime = datetime.Trim(new char[] {'}','{'});
          Log.Log(LogType.FILE, LogLevel.DEBUG, " ===========================0 " + datetime);
          DateTime dt = DateTime.Parse(datetime);
          r.Datetime = dt.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss");
          r.Description = line;
          r.CustomStr3 = line.Substring(0, line.LastIndexOf('\\'));
          r.CustomStr4 = arr[arr.Length - 1];
          r.LogName = LogName;

          WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();

          if (windowsIdentity != null)
          {
            r.UserName = windowsIdentity.Name.Split('\\')[1];
          }
          else
          {
            r.UserName = windowsIdentity.Name;
          }

          Log.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName : " + r.ComputerName);
          Log.Log(LogType.FILE, LogLevel.DEBUG, "EventType : " + r.EventType);
          Log.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory : " + r.EventCategory);
          Log.Log(LogType.FILE, LogLevel.DEBUG, "Datetime : " + r.Datetime);
          Log.Log(LogType.FILE, LogLevel.DEBUG, "Description : " + r.Description);
          Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3 : " + r.CustomStr3);

          Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4 : " + r.CustomStr4);
          Log.Log(LogType.FILE, LogLevel.DEBUG, "LogName : " + r.LogName);
          Log.Log(LogType.FILE, LogLevel.DEBUG, "UserName : " + r.UserName);

          SetRecordData(r);
        }
        catch (Exception e)
        {
          Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
          Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
          Log.Log(LogType.FILE, LogLevel.ERROR, "Line : " + line);
          return true;
        }
      }
      return true;
    } // ParseSpecific

    protected override void ParseFileNameLocal()
    {
      Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() --> Start");
      Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() --> Dir : " + Dir);

      if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
      {
        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() --> Searching for file in directory: " + Dir);
        ArrayList arrFileNames = new ArrayList();
        foreach (String file in Directory.GetFiles(Dir))
        {
          string sFile = Path.GetFileName(file).ToString();
          if (sFile.StartsWith("search-") == true)
            arrFileNames.Add(sFile);
        }

        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() --> Sorting file in directory: " + Dir);

        String[] dFileNameList = SortFiles(arrFileNames);

        //for (int i = 0; i < arrFileNames.Count; i++)
        //{
        //    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() --> arrFileNames is: " + arrFileNames[i]);
        //}

        //for (int i = 0; i < dFileNameList.Length; i++)
        //{
        //    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() --> dFileNameList is: " + dFileNameList[i]);
        //}


        if (dFileNameList.Length > 0)
        {
          if (string.IsNullOrEmpty(lastFile) == false)
          {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() --> LastFile is: " + lastFile);

            if (File.Exists(lastFile) == true)
            {

              Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() --> lastFile is not null  : " + lastFile);

              FileStream fs = new FileStream(lastFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
              BinaryReader br = new BinaryReader(fs, enc);

              br.BaseStream.Seek(Position, SeekOrigin.Begin);

              Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() --> Position is: " + br.BaseStream.Position.ToString());
              Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() --> Length is: " + br.BaseStream.Length.ToString());

              if (br.BaseStream.Position == br.BaseStream.Length - 1)
              {
                //Dosya bitmiş değiştirilecek.

                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal --> Position is at the end of the file so changing the File");

                for (int i = 0; i < dFileNameList.Length; i++)
                {
                  //Log.Log(LogType.FILE, LogLevel.DEBUG,
                  //        "ParseFileNameLocal --> dFileNameList.Length :  " + dFileNameList.Length);

                  //Log.Log(LogType.FILE, LogLevel.DEBUG,
                  //        "ParseFileNameLocal --> Dir + dFileNameList[i] :  " + Dir + dFileNameList[i]);

                  //Log.Log(LogType.FILE, LogLevel.DEBUG,
                  //        "ParseFileNameLocal --> lastFile :  " + lastFile);


                  for (int j = 0; j < dFileNameList.Length; j++)
                  {
                    Log.Log(LogType.FILE, LogLevel.DEBUG,
                            "ParseFileNameLocal --> dFileNameList :  " + dFileNameList[i]);
                  }


                  if (Dir + dFileNameList[i].ToString() == lastFile)
                  {
                    Log.Log(LogType.FILE, LogLevel.DEBUG,
                            "ParseFileNameLocal --> Dir + dFileNameList.Length : " + Dir + dFileNameList.Length);

                    if (i + 1 == dFileNameList.Length)
                    {
                      Log.Log(LogType.FILE, LogLevel.DEBUG,
                              "ParseFileNameLocal --> i : " + i + "");

                      //Daha yeni bir dosya yok. Burada bekliyoruz.
                      FileName = lastFile;
                      lastFile = FileName;
                      Log.Log(LogType.FILE, LogLevel.DEBUG,
                          "ParseFileNameLocal() --> Yeni Dosya Yok Aynı Dosyaya Devam : " + FileName);
                      break;
                    }

                    else
                    {
                      Log.Log(LogType.FILE, LogLevel.DEBUG,
                          "ParseFileNameLocal() --> Else ");

                      //Daha yeni dosyaya ekliyoruz.
                      FileName = Dir + dFileNameList[(i + 1)].ToString();
                      lastFile = FileName;
                      Position = 0;
                      Log.Log(LogType.FILE, LogLevel.DEBUG,
                          "ParseFileNameLocal() --> Yeni Dosya atandı. Lastfile : " + FileName);

                      break;
                    }
                  }
                }
              }
              else
              {
                //Dosyayı okumaya devam.

                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal --> Dosya Sonu Okunmadı Okuma Devam Ediyor");
                FileName = lastFile;
                Log.Log(LogType.FILE, LogLevel.DEBUG,
                    "ParseFileNameLocal() --> FileName = LastFile " + FileName);
              }
            }
            else
            {
              //son okunan dosya bulunamadı en yeni oluşan dosya okunmaya başlanacak.
              if (dFileNameList.Length > 0)
              {
                FileName = Dir + dFileNameList[dFileNameList.Length - 1];
                lastFile = FileName;
                Position = 0;
                Log.Log(LogType.FILE, LogLevel.DEBUG,
                       "ParseFileNameLocal() --> Lastfile directory'de bulunamadı. LastFile en yeni dosya olarak atandı : " + FileName);
              }
            }

            SetRegistry();
          }
          else
          {
            //ilk defa log okunacak.

            if (dFileNameList.Length > 0)
            {
              FileName = Dir + dFileNameList[0];
              lastFile = FileName;
              Position = 0;
              Log.Log(LogType.FILE, LogLevel.DEBUG,
                     "ParseFileNameLocal() --> İlk defa log okunacak . Lastfile en eski dosya olarak atandı : " + FileName);
            }

            SetRegistry();
            // fdsfds
          }
        }
        else
          Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameLocal() --> Directory is empty!");
      }
      else
        FileName = Dir;

      try
      {
        string[] arr = FileName.Split('-');
        //eventCategory = arr[1];
        datetime = resolveDatetime(arr[2], arr[3]);
      }
      catch (Exception e)
      {
        Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameLocal() --> unrecognized file name!.");
        DateTime dt = File.GetCreationTime(FileName);
        datetime = dt.ToString("{yyyy-MM-dd HH:mm:ss}");
      }
    } // ParseFileNameLocal

    private string resolveDatetime(string date, string time)
    {
      try
      {
        date = string.Concat(date.Substring(0, 4), "-", date.Substring(4, 2), "-", date.Substring(6, 2));
        time = string.Concat(time.Substring(0, 2), ":", time.Substring(2, 2), ":", time.Substring(4, 2));

        return string.Concat(date, " ", time);
      }
      catch (Exception e)
      {
        Log.Log(LogType.FILE, LogLevel.ERROR, "resolveDatetime() --> invalid date time format! ");
        Log.Log(LogType.FILE, LogLevel.ERROR, "resolveDatetime() --> " + e.Message);
        return "";
      }
    } // resolveDatetime

    private string[] SortFiles(ArrayList arrFileNames)
    {
#if HARD_DEBUG_MODE
      Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() --> sorting started! ");
      Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() --> filename count is " + arrFileNames.Count);
#endif

      UInt64[] dFileNumberList = new UInt64[arrFileNames.Count];
      String[] dFileNameList = new String[arrFileNames.Count];
      ArrayList NewFileName = new ArrayList();

      for (int i = 0; i < arrFileNames.Count; i++)
      {
        NewFileName.Add(arrFileNames[i]);
      }

      //for (int i = 0; i < NewFileName.Count; i++)
      //{
      //    Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() --> NewFileName : " + NewFileName[i]);
      //}

      NewFileName.Sort();

      for (int i = 0; i < NewFileName.Count; i++)
      {
        dFileNameList = new string[NewFileName.Count];
        dFileNameList = NewFileName.ToArray(typeof(string)) as string[];
      }

      //for (int i = 0; i < dFileNameList.Count(); i++)
      //{
      //    Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() --> dFileNameList : " + dFileNameList[i]);
      //}

      //try
      //{
      //    for (int i = 0; i < arrFileNames.Count; i++)
      //    {
      //        Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() --> filename : " + arrFileNames[i]);
      //        string[] parts = arrFileNames[i].ToString().Split(new char[] { '.', '-' }, StringSplitOptions.RemoveEmptyEntries);

      //        for (int j = 0; j < parts.Length; j++)
      //        {
      //            Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() --> parts : " + parts[i]);
      //        }

      //        if (parts.Length == 5)
      //        {
      //            dFileNumberList[i] = Convert.ToUInt64(parts[2] + parts[3]);

      //            #if HARD_DEBUG_MODE
      //                Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() --> file number is inserted as " + dFileNumberList[i]);
      //            #endif

      //            dFileNameList[i] = (string)arrFileNames[i];
      //        }
      //        else
      //        {
      //            //dFileNameList[i] = null;//Onur
      //            dFileNameList[i] = arrFileNames[i].ToString();
      //        }

      //    }

      //    Array.Sort(dFileNumberList, dFileNameList);


      //    Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() --> sorted files will be written down.");
      //    for (int i = 0; i < dFileNameList.Length; i++)
      //    {
      //        Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() --> " + dFileNameList[i].ToString());
      //    }
      //}
      //catch (Exception ex)
      //{
      //    Log.Log(LogType.FILE, LogLevel.ERROR, "SortFiles() --> " + ex.ToString());
      //}
      return dFileNameList;
    } // SortFiles

    protected override void ParseFileNameRemote()
    {
      throw new NotImplementedException();
    } // ParseFileNameRemote

    public override void Start()
    {
      base.Start();
    } // Start

    public override void GetFiles()
    {
      try
      {
        GetRegistry();
        Dir = GetLocation();
        Today = DateTime.Now;
        ParseFileName();
      }
      catch (Exception e)
      {
        if (reg == null)
        {
          Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "GetFiles() --> Error while getting files, Exception: " + e.Message);
          Log.Log(LogType.EVENTLOG, LogLevel.ERROR, e.StackTrace);
        }
        else
        {
          Log.Log(LogType.FILE, LogLevel.ERROR, "GetFiles() --> Error while getting files, Exception: " + e.Message);
          Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
        }
      }
    } // GetFiles
  }
}
