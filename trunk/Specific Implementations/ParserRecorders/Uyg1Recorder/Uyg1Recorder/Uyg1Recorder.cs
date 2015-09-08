//Name: Uygulama 1 Recorder
//Writer: Ali Yıldırım
//Date: 3.11.2010




//TODO SetFolderName() and SetFileName() metods will be written. 5 Kasım
//TODO SortFileName() and SortFolderName con be written.(Optional) 5 Kasım





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Log;
using System.IO;
using SharpSSH.SharpSsh;
using CustomTools;
using System.Collections;
using System.Globalization;
using System.Timers;

namespace Parser
{
    public class Uyg1Recorder : Parser
    {
        public Uyg1Recorder()
            : base()
        {
            LogName = "Uyg1Recorder";
        }

        public override void Init()
        {
            GetFiles();
        }

        public Uyg1Recorder(String fileName)
            : base(fileName)
        {
        }

        public override void Start()
        {
            base.Start();
        }

        private string localFolderName = "";
        public object[] sortedFileList;
        public string[] sortedFolderList;

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseSpecific() -->> Parsing Specific line");
            if (line == "")
                return true;

            if (!dontSend)
            {
                String[] arr = SpaceSplit(line, true);

                try
                {
                    int year = DateTime.Now.Year;
                    if (String.IsNullOrEmpty(localFolderName))
                    {
                        if (localFolderName.Split('-').Length == 2)
                        {
                            year = Convert.ToInt32(localFolderName.Split('-')[1]);
                        }
                    }

                    Rec r = new Rec();
                    r.Description = line;
                    r.Datetime = Convert.ToDateTime(arr[1] + "/" + arr[0] + "/" + year + " " + arr[2], CultureInfo.InvariantCulture).ToString("dd/MM/yyyy HH:mm:ss");
                    r.SourceName = arr[3];
                    r.EventType = "";

                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (arr[i] == "for")
                        {
                            if (arr[i + 1] != "invalid")
                            {
                                r.CustomStr2 = arr[i + 1];
                                for (int j = 5; j < i + 1; j++)
                                {
                                    r.EventType += arr[j];
                                }
                            }
                            else
                            {
                                r.CustomStr2 = arr[i + 3];
                                for (int j = 5; j < i + 3; j++)
                                {
                                    r.EventType += arr[j];
                                }
                            }
                        }
                        else if (arr[i] == "from")
                        {
                            r.CustomStr3 = arr[i + 1];
                        }
                        else if (arr[i] == "port")
                        {
                            r.CustomInt2 = Convert.ToInt32(arr[i + 1]);
                            r.CustomStr1 = arr[i + 2];
                        }
                    }

                    r.LogName = LogName;
                    r.ComputerName = Environment.MachineName;
                    SetRecordData(r);
                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  ParseSpecific() -->> Message: " + e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  ParseSpecific() -->> Stack: " + e.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  ParseSpecific() -->> Line: " + line);
                    return false;
                }
            }
            return true;
        }

        private object[] GetSortedFileList(string tempCustomVar1)
        {
            ArrayList fileNameList = new ArrayList();
            object[] sortedList = new object[1];
            try
            {
                String command = "ls " + Dir + tempCustomVar1;
                String stdOut = "";
                String stdErr = "";

                if (!se.Connected)
                    se.Connect();
                se.RunCommand(command, ref stdOut, ref stdErr);
                se.Close();
                StringReader sr = new StringReader(stdOut);
                string file = "";

                while ((file = sr.ReadLine()) != null)
                {
                    fileNameList.Add(file);
                }

                fileNameList.Sort();
                sortedList = fileNameList.ToArray();
            }
            catch (Exception ex) { }

            return sortedList;
        }

        private string SetFolderName(string folderName)
        {
            bool lastFolderFound = false;

            if (sortedFolderList.Length > 0)
            {
                if (!String.IsNullOrEmpty(folderName))
                {
                    for (int i = 0; i < sortedFolderList.Length; i++)
                    {
                        if (folderName == sortedFolderList[i])
                        {
                            lastFolderFound = true;
                            if (sortedFolderList.Length > i + 1)
                            {
                                tempCustomVar1 = sortedFolderList[i + 1];
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -> SetFolderName() -->> New folder is assigned. New file name: " + tempCustomVar1);
                                return tempCustomVar1;
                            }
                            else
                            {
                                //There is no new file to assign.
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -> SetFolderName() -->> There is no new file to assign. Continue with the same file: " + folderName);
                                return folderName;
                            }
                        }
                    }
                }
                else
                {
                    tempCustomVar1 = sortedFolderList[0];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -> SetFolderName() -->> There is no folder read before. First folder in Dir is assigned to 'tempCustomVar1': " + tempCustomVar1);
                    return tempCustomVar1;
                }
                if (!lastFolderFound)
                {
                    //Son dosya bulunamadı.
                    //Directory should be checked.
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -> SetFolderName() -->> Last folder could not found. Last Folder: " + folderName);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -> SetFolderName() -->> Directory should be checked.");
                }
            }
            else
            {
                //There is no folder in directory.
                //Directory should be checked.
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -> SetFolderName() -->> There is no folder in directory.");
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -> SetFolderName() -->> Directory should be checked.");
            }

            return folderName;
        }

        private void SetFileName(string folderName, string fileName)
        {
            sortedFileList = GetSortedFileList(folderName);
            bool lastFileFound = false;
            if (sortedFileList.Length > 0)
            {
                if (!String.IsNullOrEmpty(fileName))
                {
                    for (int i = 0; i < sortedFileList.Length; i++)
                    {
                        if (Dir + folderName + "/" + sortedFileList[i].ToString() == lastFile)
                        {
                            lastFileFound = true;
                            if (sortedFileList.Length > i + 1)
                            {
                                //Daha yeni bir dosya var. Lastfile yeni dosyaya atanacak.
                                FileName = Dir + folderName + "/" + sortedFileList[i + 1].ToString();
                                lastFile = FileName;
                                Position = 0;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -> SetFileName() -->> There is no line left to be read.  Lastfile changed to New file: " + lastFile);
                                break;
                            }
                            else
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -> SetFileName() -->> We are looking if this file is the last day of month. LastFile: " + lastFile + ", Customvar1: " + tempCustomVar1);

                                if (sortedFolderList[sortedFolderList.Length - 1] != folderName)
                                {
                                    //Sıradaki klasör atanacak.
                                    tempCustomVar1 = SetFolderName(folderName);
                                    //Klasördeki ilk dosya atanacak.
                                    SetFileName(tempCustomVar1, "");
                                }
                                else
                                {
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -> SetFileName() -->> There is no New File and new folder. Wait for log. LastFile: " + lastFile + " LastFolder: " + tempCustomVar1);
                                }
                            }
                            break;
                        }
                    }

                    if (!lastFileFound)
                    {
                        //LasT file did not found in that directory.
                        //Please check your directory.
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -> SetFileName() -->> LasT file did not found in that directory. Folder Name: " + folderName);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -> SetFileName() -->> Please check your directory!");
                    }
                }
                else
                {
                    if (sortedFileList.Length > 0)
                    {
                        FileName = Dir + folderName + "/" + sortedFileList[0];
                        lastFile = FileName;
                        Position = 0;
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -> SetFileName() -->> First file is setted in that directory. CustomVar1: " + folderName + ". We will start to read first day fo that month: Filename: " + lastFile);
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -> SetFileName() -->> There is still no file in that directory. We will set next folder. " + folderName);
                        //Sıradaki klasör atanacak.
                        tempCustomVar1 = SetFolderName(folderName);
                        //Klasördeki ilk dosya atanacak.
                        SetFileName(tempCustomVar1, "");
                    }
                }
            }
            else
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -> SetFileName() -->> There is still no file in that directory. We will set next folder: " + folderName);
                //Sıradaki klasör atanacak.
                tempCustomVar1 = SetFolderName(folderName);
                //Klasördeki ilk dosya atanacak.
                SetFileName(tempCustomVar1, "");
            }
        }

        protected override void ParseFileNameRemote()
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -->> Enter the Function.");
                String stdOut = "";
                String stdErr = "";

                se = new SshExec(remoteHost, user);
                se.Password = password;

                ArrayList folderNameList = new ArrayList();
                ArrayList fileNameList = new ArrayList();

                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -->> Searce in Directory. Dir: " + Dir);

                    String command = "ls " + Dir;
                    if (!se.Connected)
                        se.Connect();
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    se.Close();
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -->> Ssh Command Result: " + stdOut);
                    StringReader sr = new StringReader(stdOut);
                    string file = "";

                    while ((file = sr.ReadLine()) != null)
                    {
                        folderNameList.Add(file);
                    }
                    sr.Dispose();
                    sortedFolderList = SortFolders(folderNameList);

                    if (!String.IsNullOrEmpty(tempCustomVar1))
                    {
                        // daha önceden log okunmuş kaldığımız yeri bulmamız lazım.
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -->> CustomVar1 is not null. CustomVar1: " + tempCustomVar1);
                        bool bLastFolderExist = false;

                        for (int i = 0; i < sortedFolderList.Length; i++)
                        {
                            if (sortedFolderList[i].ToString() == base.tempCustomVar1)
                            {
                                bLastFolderExist = true;
                                break;
                            }
                        }
                        if (bLastFolderExist)
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -->> Month found read before. CustomVar1: " + tempCustomVar1);

                            //Bu klasör içindeki dosyaları sıralıyoruz.
                            sortedFileList = GetSortedFileList(tempCustomVar1);

                            bool bLastFileExist = false;
                            if (!String.IsNullOrEmpty(lastFile))
                            {
                                //daha önceden log okuduğumuz ayı bulduk. Şimdi kaldığımız günü de bulmamız lazım.
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -->> LastFile is not null. Lastfile: " + lastFile);
                                if (sortedFileList.Length == 0)
                                {
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -->> There is no file in directory. Change your CustomVar1 or Folder will be changed. CustomVar1: " + tempCustomVar1);
                                    //Sıradaki Klasörü atıyoruz.
                                    tempCustomVar1 = SetFolderName(tempCustomVar1);
                                    //Klasördeki ilk dosyayı atıyoruz.
                                    SetFileName(tempCustomVar1, "");
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -->> There is no file in directory. Folder changed: " + tempCustomVar1);
                                }
                                else
                                {
                                    for (int i = 0; i < sortedFileList.Length; i++)
                                    {
                                        if ((base.Dir + tempCustomVar1 + "/" + sortedFileList[i].ToString()) == base.lastFile)
                                        {
                                            bLastFileExist = true;
                                            break;
                                        }
                                    }

                                    if (bLastFileExist)
                                    {
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -->> LastFile found read before. Lastfile: " + lastFile);

                                        if (CheckPositionInFile(lastFile) > 1)
                                        {
                                            //Dosyada okuyacak satır var ise okumaya devam edeceğiz.
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -->> There is still line to be read. Continue to Lastfile: " + lastFile);
                                            FileName = lastFile;
                                        }
                                        else
                                        {
                                            //Sıradaki dosyayı atıyoruz.
                                            SetFileName(tempCustomVar1, lastFile);
                                        }
                                    }
                                    else
                                    {
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -->> LastFile is not found in that Directory. Lastfile will be assigned first file in Folder: " + tempCustomVar1);
                                        //Klasördeki ilk dosyayı atıyoruz.
                                        SetFileName(tempCustomVar1, "");
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -->> New file to be read: " + lastFile);
                                    }
                                }
                            }
                            else
                            {
                                //Bu ay için hiç log alınmamış. İlk günden log almaya başlayacağız
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -->> There is any file read before in that directory. We will read first file.");
                                //Klasördeki ilk dosyayı atıyoruz.
                                SetFileName(tempCustomVar1, "");
                            }
                        }
                        else
                        {
                            // okunan son dosya directory'de bulunamadı. Kullanıcının directory'i kontrol etmesini isteriz.
                            //Yada o directorydeki ilk dosyayı atayabiliriz. İsteğe göre yazılacak.
                        }
                    }
                    else
                    {
                        //İlk defa log okuma işlemi yapılacak. İlk klasörün ilk dosyasını okuyacağız.
                        //Dir'deki ilk dosyayı atıyoruz.
                        tempCustomVar1 = SetFolderName("");
                        //Klasördeki ilk dosyayı atıyoruz.
                        SetFileName(tempCustomVar1, "");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -->> Service is started first time or CustomVar1 and LastFile are initilized to null.");
                    }
                }
                else
                {
                    FileName = Dir;
                    lastFile = FileName;
                    Position = 0;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameRemote() -->> Directory assigned to a file. Filename: " + lastFile);
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "  ParseFileNameRemote() -->> Error on finding LastFile. Message: " + ex.Message.ToString());
                Log.Log(LogType.FILE, LogLevel.ERROR, "  ParseFileNameRemote() -->> Error on finding LastFile. Trace: " + ex.StackTrace);
                Log.Log(LogType.FILE, LogLevel.ERROR, "  ParseFileNameRemote() -->> Last Values: LastFile: " + lastFile + ", Customvar1: " + tempCustomVar1 + ", Dir: " + Dir);
            }
            localFolderName = tempCustomVar1;
        }

        private string[] SortFolders(ArrayList arrFolderNames)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "  SortFiles() -->> Klasör isimleri sıralanıyor. ");

            UInt64[] dFileNumberList = new UInt64[arrFolderNames.Count];
            String[] dFileNameList = new String[arrFolderNames.Count];

            try
            {
                for (int i = 0; i < arrFolderNames.Count; i++)
                {
                    string[] arr = arrFolderNames[i].ToString().Split('-');
                    if (arr.Length == 2)
                    {
                        dFileNumberList[i] = Convert.ToUInt64(arr[0] + arr[1]);
                        dFileNameList[i] = arrFolderNames[i].ToString();
                    }
                }

                Array.Sort(dFileNumberList, dFileNameList);
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "  SortFiles() -->> Numaralama döngüsü. Mesaj: " + ex.ToString());
            }

            return dFileNameList;
        }

        private int CheckPositionInFile(string lastFile)
        {
            int lineCount = 0;
            try
            {
                string stdOut = "";
                string stdErr = "";
                String commandRead;
                StringReader stReader;
                String line = "";

                if (readMethod == "nread")
                {
                    commandRead = "nread" + " -n " + Position + "," + 3 + "p " + lastFile;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  CheckPositionInFile() -->> commandRead For nread Is : " + commandRead);

                    if (!se.Connected)
                        se.Connect();
                    se.RunCommand(commandRead, ref stdOut, ref stdErr);
                    if (se.Connected)
                        se.Close();

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  CheckPositionInFile() -->> commandRead'den dönen strOut : " + stdOut);

                    stReader = new StringReader(stdOut); //lastFile'dan line ve pozisyon okundu ve şimdi test ediliyor. 
                    while ((line = stReader.ReadLine()) != null)
                    {
                        lineCount++;
                    }
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  CheckPositionInFile() -->> Okunacak satır sayısı bulundu. En az: " + lineCount);
                }
                else
                {
                    commandRead = "sed" + " -n " + Position + "," + (Position + 2) + "p " + lastFile;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  CheckPositionInFile() -->> commandRead For sed Is : " + commandRead);

                    if (!se.Connected)
                        se.Connect();

                    se.RunCommand(commandRead, ref stdOut, ref stdErr);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  CheckPositionInFile() -->> commandRead'den dönen strOut : " + stdOut);
                    stReader = new StringReader(stdOut);

                    while ((line = stReader.ReadLine()) != null)
                    {
                        lineCount++;
                    }
                }

                return lineCount;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "  CheckPositionInFile() -->> " + lastFile + " dosyasından last line ararken problem ile karşılaşıldı.");
                Log.Log(LogType.FILE, LogLevel.ERROR, "  CheckPositionInFile() -->> Hata Mesajı: " + ex.ToString());
            }
            finally
            {
                if (se.Connected)
                    se.Close();
            }
            return lineCount;
        }

        protected override void ParseFileNameLocal()
        {

        }

        public override void GetFiles()
        {
            try
            {
                Dir = GetLocation();
                GetRegistry();
                Today = DateTime.Now;
                ParseFileName();
            }
            catch (Exception e)
            {
                if (reg == null)
                {
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  Error while getting files, Exception: " + e.Message);
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, e.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  Error while getting files, Exception: " + e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                }
            }
        }

        protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Log.Log(LogType.FILE, LogLevel.ERROR, "  dayChangeTimer_Elapsed() in Uyg1Recorder -->> Enter the Function.");
            Log.Log(LogType.FILE, LogLevel.ERROR, "  dayChangeTimer_Elapsed() in Uyg1Recorder -->> ParseFileName() method should be trigerred. ");

            ParseFileName();
        }
    }
}