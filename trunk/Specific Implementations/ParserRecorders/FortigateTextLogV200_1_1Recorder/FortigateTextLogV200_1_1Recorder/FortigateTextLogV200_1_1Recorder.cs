/*
 Write Onur Sarıkaya
 * Şahan savaşan
 * su yapı
 */

using System;
using System.Globalization;
using System.IO;
using Log;
using CustomTools;
using SharpSSH.SharpSsh;
using System.Collections;

namespace Parser
{
    public class FortigateTextLogV200_1_1Recorder : Parser
    {
        public FortigateTextLogV200_1_1Recorder()
            : base()
        {
            LogName = "FortigateTextLogV200_1_1Recorder";
            usingKeywords = false;
            lineLimit = 50;
        }

        public FortigateTextLogV200_1_1Recorder(String fileName)
            : base(fileName)
        {

        }

        public override void Init()
        {
            GetFiles();
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Line Is " + line);

            if (string.IsNullOrEmpty(line))
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Line is Null Or Empty");
                return true;
            }

            if (!dontSend)
            {

                string[] FieldArray = SpaceSplit(line, false);
                CustomBase.Rec rec = new CustomBase.Rec();
                rec.LogName = LogName;

                string myDateString = "";
                string myTimeString = "";




                for (int i = 0; i < FieldArray.Length; i++)
                {
                    if (FieldArray[i].StartsWith("date"))
                    {
                        try
                        {
                            myDateString = LineSplit(FieldArray[i]);
                        }
                        catch (Exception exception)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "ComputerName " + exception.Message);
                        }
                    }

                    if (FieldArray[i].StartsWith("time"))
                    {
                        try
                        {
                            myTimeString = LineSplit(FieldArray[i]);
                        }
                        catch (Exception exception)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "ComputerName " + exception.Message);
                        }
                    }

                    try
                    {
                        string myDateTimeString = myDateString + " " + myTimeString;
                        DateTime dt = Convert.ToDateTime(myDateTimeString);
                        rec.Datetime = dt.ToString("yyyy-MM-dd HH:mm:ss");

                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "dateTime Error " + exception.Message);
                    }


                    if (CaptionSplit(FieldArray[i]) == "devname")
                    {
                        try
                        {
                            rec.ComputerName = LineSplit(FieldArray[i].Replace('"', ' ').Trim());

                        }
                        catch (Exception exception)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "ComputerName " + exception.Message);
                        }
                    }

                    if (CaptionSplit(FieldArray[i]) == "type")
                    {
                        try
                        {
                            rec.EventCategory = LineSplit(FieldArray[i].Replace('"', ' ').Trim());

                        }
                        catch (Exception exception)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "EventCategory " + exception.Message);
                        }
                    }

                    if (CaptionSplit(FieldArray[i]) == "subtype")
                    {
                        try
                        {
                            rec.EventType = LineSplit(FieldArray[i].Replace('"', ' ').Trim());

                        }
                        catch (Exception exception)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "EventType " + exception.Message);
                        }
                    }

                    if (CaptionSplit(FieldArray[i]) == "dst_int")
                    {
                        try
                        {
                            rec.CustomStr10 = LineSplit(FieldArray[i].Replace('"', ' ').Trim());

                        }
                        catch (Exception exception)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr10 " + exception.Message);
                        }
                    }

                    if (CaptionSplit(FieldArray[i]) == "src_int")
                    {
                        try
                        {
                            rec.CustomStr9 = LineSplit(FieldArray[i].Replace('"', ' ').Trim());

                        }
                        catch (Exception exception)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr9 " + exception.Message);
                        }
                    }

                    if (CaptionSplit(FieldArray[i]) == "status")
                    {
                        try
                        {
                            rec.CustomStr2 = LineSplit(FieldArray[i].Replace('"', ' ').Trim());

                        }
                        catch (Exception exception)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr2 " + exception.Message);
                        }
                    }

                    if (CaptionSplit(FieldArray[i]) == "dst_country")
                    {
                        try
                        {
                            rec.CustomStr5 = LineSplit(FieldArray[i].Replace('"', ' ').Trim());

                        }
                        catch (Exception exception)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr5 " + exception.Message);
                        }
                    }

                    if (CaptionSplit(FieldArray[i]) == "service")
                    {
                        try
                        {
                            rec.CustomStr6 = LineSplit(FieldArray[i].Replace('"', ' ').Trim());

                        }
                        catch (Exception exception)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr6 " + exception.Message);
                        }
                    }

                    if (CaptionSplit(FieldArray[i]) == "msg")
                    {
                        try
                        {
                            rec.CustomStr7 = LineSplit(FieldArray[i].Replace('"', ' ').Trim());

                        }
                        catch (Exception exception)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr7 " + exception.Message);
                        }
                    }

                    if (CaptionSplit(FieldArray[i]) == "proto")
                    {
                        try
                        {
                            rec.CustomStr8 = LineSplit(FieldArray[i].Replace('"', ' ').Trim());

                        }
                        catch (Exception exception)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr8 " + exception.Message);
                        }
                    }

                    if (CaptionSplit(FieldArray[i]) == "src")
                    {
                        try
                        {
                            rec.CustomStr3 = LineSplit(FieldArray[i].Replace('"', ' ').Trim());

                        }
                        catch (Exception exception)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3 " + exception.Message);
                        }
                    }

                    if (CaptionSplit(FieldArray[i]) == "dst")
                    {
                        try
                        {
                            rec.CustomStr4 = LineSplit(FieldArray[i].Replace('"', ' ').Trim());

                        }
                        catch (Exception exception)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr4 " + exception.Message);
                        }
                    }

                    if (CaptionSplit(FieldArray[i]) == "dst_port")
                    {
                        try
                        {
                            rec.CustomInt4 = Convert.ToInt32(LineSplit(FieldArray[i]));

                        }
                        catch (Exception exception)
                        {
                            rec.CustomInt4 = 0;
                            Log.Log(LogType.FILE, LogLevel.INFORM, "CustomInt4 = 0 ");
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt4 " + exception.Message);
                        }
                    }

                    if (CaptionSplit(FieldArray[i]) == "src_port")
                    {
                        try
                        {
                            rec.CustomInt3 = Convert.ToInt32(LineSplit(FieldArray[i]));

                        }
                        catch (Exception exception)
                        {
                            rec.CustomInt3 = 0;
                            Log.Log(LogType.FILE, LogLevel.INFORM, "CustomInt3 = 0 ");
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt3 " + exception.Message);
                        }
                    }

                    if (CaptionSplit(FieldArray[i]) == "log_id")
                    {
                        try
                        {
                            rec.CustomInt2 = Convert.ToInt32(LineSplit(FieldArray[i]));

                        }
                        catch (Exception exception)
                        {
                            rec.CustomInt2 = 0;
                            Log.Log(LogType.FILE, LogLevel.INFORM, "CustomInt2 = 0 ");
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt2 " + exception.Message);
                        }
                    }
                    //policyid

                    if (CaptionSplit(FieldArray[i]) == "policyid")
                    {
                        try
                        {
                            rec.CustomInt1 = Convert.ToInt32(LineSplit(FieldArray[i]));

                        }
                        catch (Exception exception)
                        {
                            rec.CustomInt1 = 0;
                            Log.Log(LogType.FILE, LogLevel.INFORM, "CustomInt1 = 0 ");
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt1 " + exception.Message);
                        }
                    }

                    if (CaptionSplit(FieldArray[i]) == "sent")
                    {
                        try
                        {
                            rec.CustomInt5 = Convert.ToInt32(LineSplit(FieldArray[i]));

                        }
                        catch (Exception exception)
                        {
                            rec.CustomInt5 = 0;
                            Log.Log(LogType.FILE, LogLevel.INFORM, "CustomInt5 = 0 ");
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt5 " + exception.Message);
                        }
                    }

                    if (CaptionSplit(FieldArray[i]) == "rcvd")
                    {
                        try
                        {
                            rec.CustomInt6 = Convert.ToInt32(LineSplit(FieldArray[i]));

                        }
                        catch (Exception exception)
                        {
                            rec.CustomInt6 = 0;
                            Log.Log(LogType.FILE, LogLevel.INFORM, "CustomInt6 = 0 ");
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt6 " + exception.Message);
                        }
                    }
                }

                Log.Log(LogType.FILE, LogLevel.DEBUG, "myDateString: " + rec.ComputerName);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "myTimeString: " + rec.ComputerName);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Datetime: " + rec.Datetime);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt6: " + rec.CustomInt6);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt5: " + rec.CustomInt5);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1: " + rec.CustomInt1);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2: " + rec.CustomInt2);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3: " + rec.CustomInt3);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4: " + rec.CustomInt4);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName: " + rec.ComputerName);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + rec.EventType);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr10: " + rec.CustomStr10);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9: " + rec.CustomStr9);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5: " + rec.CustomStr5);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + rec.CustomStr6);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + rec.CustomStr7);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8: " + rec.CustomStr8);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);


                try
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "REcord is sending now.");
                    SetRecordData(rec);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "REcord is sended.");
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Record send error." + exception);
                }

                bool logtype = false;
                bool usertype = false;
                try
                {


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

        public string LineSplit(string value)
        {
            string returnValue = "";

            try
            {
                if (value.Contains("="))
                {
                    returnValue = value.Split('=')[1].ToString();
                }
            }
            catch (Exception exception)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Enter the Function.");
            }

            return returnValue;
        } // LineSplit

        public string CaptionSplit(string value)
        {
            string returnValue = "";

            try
            {
                if (value.Contains("="))
                {
                    returnValue = value.Split('=')[0].ToString();
                }
            }
            catch (Exception exception)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Enter the Function.");
            }

            return returnValue;
        } // LineSplit

        //protected override void ParseFileNameRemote()
        //{
        //    string line = "";
        //    String stdOut = "";
        //    String stdErr = "";

        //    try
        //    {
        //        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Enter the Function.");
        //        se = new SshExec(remoteHost, user);
        //        se.Password = password;
        //        if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
        //        {
        //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Home Directory | " + Dir);
        //            String command = "ls -lt " + Dir + " | grep radius_";
        //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH command : " + command);

        //            se.Connect();
        //            se.RunCommand(command, ref stdOut, ref stdErr);
        //            se.Close();

        //            StringReader sr = new StringReader(stdOut);
        //            ArrayList arrFileNameList = new ArrayList();
        //            while ((line = sr.ReadLine()) != null)
        //            {
        //                String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //                if (arr[arr.Length - 1].StartsWith("radius_") == true && arr[arr.Length - 1].Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries).Length <= 3)//Name changed
        //                {
        //                    arrFileNameList.Add(arr[arr.Length - 1]);
        //                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Dosya ismi okundu: " + arr[arr.Length - 1]);
        //                }
        //            }

        //            String[] dFileNameList = SortFiles(arrFileNameList);
        //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> arrayFileNameList'e atılan dosya isimleri sıralandı.");

        //            if (!String.IsNullOrEmpty(lastFile))
        //            {
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile is not null: " + lastFile);
        //                bool bLastFileExist = false;

        //                for (int i = 0; i < dFileNameList.Length; i++)
        //                {
        //                    if ((base.Dir + dFileNameList[i].ToString()) == base.lastFile)
        //                    {
        //                        bLastFileExist = true;
        //                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile is found: " + lastFile);
        //                        break;
        //                    }
        //                }

        //                if (bLastFileExist)
        //                {
        //                    if (Is_File_Finished(lastFile))
        //                    {
        //                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Last File is finished. Previous File: " + lastFile);

        //                        for (int i = 0; i < dFileNameList.Length; i++)
        //                        {
        //                            if (Dir + dFileNameList[i].ToString() == lastFile)
        //                            {
        //                                if (dFileNameList.Length > i + 1)
        //                                {
        //                                    FileName = Dir + dFileNameList[i + 1].ToString();
        //                                    Position = 0;
        //                                    lastFile = FileName;
        //                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> New File is assigned. New File: " + FileName);
        //                                    break;
        //                                }
        //                                else
        //                                {
        //                                    FileName = lastFile;
        //                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> There is no new file to assign. Wait this file for log: " + FileName);
        //                                }
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        FileName = lastFile;
        //                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> There is still line in lastfile.  Continue to read this file: " + FileName);
        //                    }
        //                }
        //                else
        //                {
        //                    FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString(CultureInfo.InvariantCulture);
        //                    Position = 0;
        //                    lastFile = FileName;
        //                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile Silinmis , Dosya Bulunamadı.  Yeni File : " + FileName);
        //                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Start to read  main file from beginning: " + FileName);
        //                }
        //            }
        //            else
        //            {
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Last File Is Null");
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> ilk defa log okunacak.");

        //                if (dFileNameList.Length > 0)
        //                {
        //                    FileName = Dir + dFileNameList[0].ToString();
        //                    lastFile = FileName;
        //                    Position = 0;
        //                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> FileName ve LastFile en eski dosya olarak ayarlandý: " + lastFile);
        //                }
        //                else
        //                {
        //                    Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> In The Log Location There Is No Log File to read.");
        //                }
        //            }
        //        }
        //        else
        //        {
        //            FileName = Dir;
        //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Directory file olarak gösterildi.: " + FileName);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Dosya isimleri getirilirken problemle karþýlaþýldý.");
        //        Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Hata Mesajý: " + ex.ToString());
        //    }
        //}

        protected override void ParseFileNameRemote()
        {
            string line = "";
            String stdOut = "";
            String stdErr = "";

            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Enter the Function.");
                se = new SshExec(remoteHost, user);
                se.Password = password;
                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Home Directory | " + Dir);
                    String command = "ls -lt " + Dir + " | grep messages";
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH command : " + command);

                    try
                    {
                        se.Connect();
                        se.RunCommand(command, ref stdOut, ref stdErr);
                        se.Close();
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Exception : " + exception);
                    }

                    StringReader sr = new StringReader(stdOut);
                    ArrayList arrFileNameList = new ArrayList();
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (!line.Contains("."))
                        {
                            arrFileNameList.Add(line);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Dosya ismi okundu: " + line);
                        }
                        String[] arr = line.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                        if (arr[arr.Length - 1].StartsWith("messages") == true && arr[arr.Length - 1].Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries).Length <= 3)//Name changed
                        {
                            arrFileNameList.Add(arr[arr.Length - 1]);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Dosya ismi okundu: " + arr[arr.Length - 1]);
                        }
                    }//

                    String[] dFileNameList = SortFiles(arrFileNameList);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> arrayFileNameList'e atılan dosya isimleri sıralandı.");

                    if (!String.IsNullOrEmpty(lastFile))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile is not null: " + lastFile);
                        bool bLastFileExist = false;

                        for (int i = 0; i < dFileNameList.Length; i++)
                        {
                            if ((base.Dir + dFileNameList[i].ToString()) == base.lastFile)
                            {
                                bLastFileExist = true;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile is found: " + lastFile);
                                break;
                            }
                        }

                        if (bLastFileExist)
                        {
                            if (Is_File_Finished(lastFile))
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Last File is finished. Previous File: " + lastFile);

                                for (int i = 0; i < dFileNameList.Length; i++)
                                {
                                    if (Dir + dFileNameList[i].ToString() == lastFile)
                                    {
                                        if (dFileNameList.Length > i + 1)
                                        {
                                            FileName = Dir + dFileNameList[i + 1].ToString();
                                            Position = 0;
                                            lastFile = FileName;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> New File is assigned. New File: " + FileName);
                                            break;
                                        }
                                        else
                                        {
                                            FileName = lastFile;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> There is no new file to assign. Wait this file for log: " + FileName);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                FileName = lastFile;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> There is still line in lastfile.  Continue to read this file: " + FileName);
                            }
                        }
                        else
                        {
                            FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString(CultureInfo.InvariantCulture);
                            Position = 0;
                            lastFile = FileName;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile Silinmis , Dosya Bulunamadı.  Yeni File : " + FileName);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Start to read  main file from beginning: " + FileName);
                        }
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Last File Is Null");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> ilk defa log okunacak.");

                        if (dFileNameList.Length > 0)
                        {
                            FileName = Dir + dFileNameList[0].ToString();
                            lastFile = FileName;
                            Position = 0;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> FileName ve LastFile en eski dosya olarak ayarlandý: " + lastFile);
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> In The Log Location There Is No Log File to read.");
                        }
                    }
                }
                else
                {
                    FileName = Dir;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Directory file olarak gösterildi.: " + FileName);
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Dosya isimleri getirilirken problemle karþýlaþýldý.");
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Hata Mesajý: " + ex.ToString());
            }
        }

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
        } // Is_File_Finished

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
        } // SortFiles

        //protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    Log.Log(LogType.FILE, LogLevel.ERROR, "  dayChangeTimer_Elapsed() in FreeRadiusRecorder -->> Enter the Function.");
        //    Log.Log(LogType.FILE, LogLevel.ERROR, "  dayChangeTimer_Elapsed() in FreeRadiusRecorder -->> ParseFileName() method should be trigerred. ");
        //    ParseFileName();
        //}

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
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "Error while getting files, Exception: " + e.Message);
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, e.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Error while getting files, Exception: " + e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                }
            }
        } // GetFiles
    }
}
