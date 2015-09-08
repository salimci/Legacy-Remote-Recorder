/* Recorder Gazi Üniverisitesi Demo kurulumu kapsamında Fatih Sakar tarafından istenmiştir.
 * Recorder tipi Remote recorder olup belirtilen path'den tek bir file okunacak şekilde düzenlenmiştir.
 * 
 * Developed by Onur Sarıkaya
 * Date 27.06.2012 
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Log;
using CustomTools;
using SharpSSH.SharpSsh;
using System.Collections;


namespace Parser
{
    /// <summary>
    /// Okunacak log dosyasında bir log'a ait birkaç satır log bulunmakta.
    /// bu sebeple belli bir karaktere kadar olan satırlar "Fullline" da birleştirilip daha sonra Parse edilmekte.
    /// Birleştirme işlemi bittikten  sonra ise "LineEnd" bool değişkeni true olup database'e kayıt işlemi gerçekleşiyor.
    /// </summary>
    public struct Fields
    {
        public string ConnectionId,
                      EVENTCATEGORY,
                      EVENTTYPE,
                      DATETIME,
                      CUSTOMSTR1,
                      CUSTOMSTR3,
                      CUSTOMSTR4,
                      CUSTOMSTR5,
                      CUSTOMSTR6,
                      CUSTOMSTR7,
                      DESCRIPTION,
                      COMPUTER_Name;

        public string FullLine;
        public bool IsInsertData;
        public bool LineEnd;
    }

    public class LinuxDhcpRecorder : Parser
    {

        private Fields RecordFields;

        public LinuxDhcpRecorder()
            : base()
        {
            LogName = "LinuxDhcpRecorder";
            usingKeywords = false;
            lineLimit = 50;
        }

        public LinuxDhcpRecorder(String fileName)
            : base(fileName)
        {

        }

        public override void Init()
        {
            RecordFields = new Fields();
            GetFiles();
        }

        /// <summary>
        /// Loglar Parse ediliyor.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="dontSend"></param>
        /// <returns></returns>
        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Line Is " + line);

            if (!line.StartsWith("}"))
            {
                RecordFields.FullLine += line.Replace(";", " ");
            }
            else
            {
                RecordFields.LineEnd = true;
            }

            String[] stringArray = RecordFields.FullLine.Split(' ');

            if (RecordFields.LineEnd)
            {
                try
                {
                    CustomBase.Rec rec = new CustomBase.Rec();
                    rec.LogName = LogName;
                    for (int i = 0; i < stringArray.Length; i++)
                    {
                        if (stringArray[i] == "lease")
                        {
                            rec.EventType = stringArray[i];
                            rec.CustomStr3 = stringArray[i + 1];
                        }

                        else if (stringArray[i] == "hardware")
                        {
                            rec.EventCategory = stringArray[i + 2];
                        }
                        else if (stringArray[i] == "client-hostname")
                        {
                            rec.ComputerName = stringArray[i + 1];
                        }
                        else if (stringArray[i] == "starts")
                        {
                            DateTime dt = Convert.ToDateTime(stringArray[i + 2] + " " + stringArray[i + 3]);
                            rec.Datetime = dt.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        else if (stringArray[i] == "ends")
                        {
                            rec.CustomStr5 = stringArray[i + 2] + " " + stringArray[i + 3];
                        }
                        else if (stringArray[i] == "tstp")
                        {
                            rec.CustomStr6 = stringArray[i + 2] + " " + stringArray[i + 3];
                        }

                        else if (stringArray[i] == "uid")
                        {
                            rec.CustomStr7 = stringArray[i + 1];
                        }
                        rec.Description = RecordFields.FullLine;

                        Log.Log(LogType.FILE, LogLevel.DEBUG, "EVENTCATEGORY: " + rec.EventCategory);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "EVENTTYPE: " + rec.EventType);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "DATETIME: " + rec.Datetime);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "COMPUTER_Name: " + rec.ComputerName);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CUSTOMSTR3: " +rec.CustomStr3);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CUSTOMSTR5: " + rec.CustomStr5);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CUSTOMSTR6: " + rec.CustomStr6);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CUSTOMSTR7: " + rec.CustomStr7);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Description: " + rec.Description);

                        Log.Log(LogType.FILE, LogLevel.DEBUG, " Line parsing finished. Start sending data.");
                        SetRecordData(rec);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " Line parsing finished. Finished sending data.");
                        ClearRecordFields();
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " Line parsing finished. Clear Fields.");
                    }
                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "ParseSpecific : " + e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "ParseSpecific : " + e.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "ParseSpecific, Line : " + line);
                }
            }
            return true;
        } // ParseSpecific

        public void ClearRecordFields()
        {
            RecordFields.EVENTTYPE = "";
            RecordFields.EVENTCATEGORY = "";
            RecordFields.COMPUTER_Name = "";
            RecordFields.CUSTOMSTR3 = "";
            RecordFields.CUSTOMSTR4 = "";
            RecordFields.CUSTOMSTR5 = "";
            RecordFields.CUSTOMSTR6 = "";
            RecordFields.CUSTOMSTR7 = "";
            RecordFields.LineEnd = false;
        }

        protected override void ParseFileNameRemote()
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> is STARTED");
            try
            {
                se = new SshExec(remoteHost, user);
                se.Password = password;
                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> Searching files in directory : " + Dir);
                    List<String> fileNameList = GetFileNamesOnRemote();
                    fileNameList = SortFileNames(fileNameList);
                    //SetLastFile(fileNameList);
                }
                else
                {
                    FileName = Dir;
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameRemote() -->> An eror occurred : " + ex.ToString());
            }
        } // ParseFileNameRemote

        /// <summary>
        /// Gets the file names on the given directory
        /// </summary>
        /// <returns>Returned file names</returns>
        private List<String> GetFileNamesOnRemote()
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnRemote() -->> is STARTED ");

                String line = "";
                String stdOut = "";
                String stdErr = "";

                String command = "ls -lt " + Dir;//FileNames contains what.*** fileNameFilter
                Log.Log(LogType.FILE, LogLevel.DEBUG, "GetFileNamesOnRemote() -->> SSH command : " + command);

                se.Connect();
                se.RunCommand(command, ref stdOut, ref stdErr);
                se.Close();

                StringReader stringReader = new StringReader(stdOut);
                List<String> fileNameList = new List<String>();
                Boolean foundAnyFile = false;

                while ((line = stringReader.ReadLine()) != null)
                {
                    String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    fileNameList.Add(arr[arr.Length - 1]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "GetFileNamesOnRemote() -->> File name is read: " + arr[arr.Length - 1]);
                    foundAnyFile = true;
                }

                if (!foundAnyFile)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "GetFileNamesOnRemote() -->> There is no proper file in directory");
                }

                Log.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnRemote() -->> is successfully FINISHED");
                return fileNameList;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " GetFileNamesOnRemote() -->> An error occurred :" + ex.ToString());
                return null;
            }
        } // GetFileNamesOnRemote

        private List<String> SortFileNames(List<String> fileNameList)
        {
            foreach (string t in fileNameList)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() " + t);
            }
            Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is STARTED ");
            return fileNameList;
        } // SortFileNames

        private bool Is_File_Finished(string file)
        {
            int lineCount = 0;
            string stdOut = "";
            string stdErr = "";
            String commandRead;
            StringReader stReader;
            String line = "";

            try
            {
                if (readMethod == "nread")
                {
                    commandRead = "nread" + " -n " + Position + "," + 3 + "p " + file;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead For nread Is : " + commandRead);

                    se.Connect();
                    se.RunCommand(commandRead, ref stdOut, ref stdErr);
                    se.Close();

                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead'den dönen strOut : " + stdOut);

                    stReader = new StringReader(stdOut);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Okunacak satýr sayýsýna bakýlýyor.");
                    //lastFile'dan line ve pozisyon okundu ve þimdi test ediliyor. 
                    while ((line = stReader.ReadLine()) != null)
                    {
                        if (line.StartsWith("~?`Position"))
                        {
                            continue;
                        }
                        lineCount++;
                    }
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Okunacak satýr sayýsý bulundu. En az: " + lineCount);
                }
                else
                {
                    commandRead = "sed" + " -n " + Position + "," + (Position + 2) + "p " + file;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead For nread Is : " + commandRead);

                    se.Connect();
                    se.RunCommand(commandRead, ref stdOut, ref stdErr);
                    se.Close();

                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead'den dönen strOut : " + stdOut);

                    stReader = new StringReader(stdOut);

                    while ((line = stReader.ReadLine()) != null)
                    {
                        lineCount++;
                    }
                }

                if (lineCount > 0)
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> " + lastFile + " dosyasının sonu aranırken problem ile karşılaşıldı.");
                Log.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> Hata Mesajı: " + ex.ToString());
                Log.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> " + lastFile + " dosyasını değiştirmeden devam edeceğiz.");
                return false;
            }
        }

        private string[] SortFiles(ArrayList arrFileNames)
        {
            UInt64[] dFileNumberList = new UInt64[arrFileNames.Count];
            String[] dFileNameList = new String[arrFileNames.Count];

            try
            {
                for (int i = 0; i < arrFileNames.Count; i++)
                {
                    string[] parts = arrFileNames[i].ToString().Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3)
                    {
                        dFileNumberList[i] = Convert.ToUInt64(parts[1]);
                        dFileNameList[i] = arrFileNames[i].ToString();
                    }
                    else
                    {
                        dFileNameList[i] = null;
                    }
                }

                Array.Sort(dFileNumberList, dFileNameList);

                Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> Sýralanmýþ dosya isimleri yazýlýyor.");
                for (int i = 0; i < dFileNameList.Length; i++)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> " + dFileNameList[i]);
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "SortFiles() -->> Sıralama işlemi. Mesaj: " + ex.ToString());
            }

            return dFileNameList;
        }

        public override void GetFiles()
        {
            Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "Starting GetFiles.");
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
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "Error while getting files, Exception: " + e.Message);
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, e.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Error while getting files, Exception: " + e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                }
            }
        }
    }
}
