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
using System.Collections;

namespace Parser
{
    public class TelekomRecorder : Parser
    {
        public TelekomRecorder()
            : base()
        {
            LogName = "TelekomRecorder";
            usingKeywords = false;
            lineLimit = 50;
        }

        public override void Init()
        {
            GetFiles();
        }

        public TelekomRecorder(String fileName)
            : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            if (line == "")
                return true;
            if (!dontSend)
            {
                String[] arr = SpaceSplit(line, false, '"');
                try
                {
                    Rec r = new Rec();

                    string[] fields = line.Split('@');

                    for (int i = 0; i < fields.Length; i++)
                    {
                        fields[i] = fields[i].Trim('#');
                    }

                    try
                    {
                        //ALLOW#@#Gambling Related#@#Sat Nov 20 00:00:04 EET 2010#@#null#@#null#@#null#@#085.109.179.003#@#mssp_ww#@#www.mackolik.com#@#http://www.mackolik.com/LiveScores/SequenceNo.aspx#@#-
                        //#Sat Nov 20 00:00:04 EET 2010#
                        //AOW#@#Search Engines#@#Sat gn 01  02:12:3aclk?saaclk?sa=l% EEf=CErAPuW4eTfbEGpG4hQeO2eiAB--IqJkBqbjrkxDZpMzlARAEKAhQueCHiAJglladhoAhoAHdg_D-A8gBAaoEFk_Q8kKy0eJhL1ltT08nmVqCuykABJwullnum=4%llsig=AGiWqtyIkw--v0IyoK-GySWz_TXKibv4cg.04ad#http:ekJan01  otelgumuslukproxl#@#np@#085.1Co109n0.236#e6RID79.093:36:3i4:06:1FQjCNGtPoh6iH6DqUwHB9nVXMOOkwzL7A%26204#52
                        //AOW#@#Search Engines#@#Sa Jan01  02:12:3aclk?sa2ht?sa=t/56sDweb%=0eb%llcd=2%5Bv/d=0CB8QFjAB.0ssp_ww#@#null#@#http:32582bt.com/index/mymail.ht.tm52
                        string[] datearr = fields[2].Split(' ');
                        string tempdate = datearr[2] + "/" + datearr[1] + "/" + datearr[5].TrimEnd('#') + " " + datearr[3];
                        DateTime date_time = Convert.ToDateTime(tempdate, CultureInfo.InvariantCulture);
                        r.Datetime = date_time.ToString("yyyy/MM/dd HH:mm:ss");

                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Datetime = " + r.Datetime);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "In Catch 1 " + ex.Message);
                    }

                    r.EventType = fields[0];
                    r.CustomStr5 = fields[1];
                    r.CustomStr3 = fields[6];
                    r.CustomStr9 = fields[8];
                    if (fields[9].Length > 898)
                    {
                        r.Description = fields[9].Substring(0, 898);
                    }
                    else
                        r.Description = fields[9];
                    r.LogName = LogName;
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
        }

        protected override void ParseFileNameLocal() // last file ve position
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, " TelekomRecorder In ParseFileNameLocal() -->> Enter The Function");

            FileStream fs = null;
            BinaryReader br = null;
            Int64 currentPosition = Position;

            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  TelekomRecorder In ParseFileNameLocal() -->> Searching In Directory : " + Dir);

                ArrayList filenameList = new ArrayList();

                foreach (String file in Directory.GetFiles(Dir))
                {
                    if (Path.GetFileName(file).StartsWith("LogArchiveCF"))
                    {
                        filenameList.Add(Path.GetFileName(file));
                    }
                }

                Log.Log(LogType.FILE, LogLevel.DEBUG, "  TelekomRecorder In ParseFileNameLocal() -->> " + filenameList.Count.ToString() + " File Found ");

                string[] fullfileName = SortFiles(filenameList);

                Log.Log(LogType.FILE, LogLevel.INFORM, "Last File Is : " + lastFile);

                if (String.IsNullOrEmpty(lastFile))
                {
                    if (fullfileName.Length > 0)
                    {
                        FileName = Dir + fullfileName[0].ToString();
                        lastFile = FileName;
                        Log.Log(LogType.FILE, LogLevel.INFORM, " TelekomRecorder In ParseFileNameLocal() -->> Last File Is Null So Last File Setted The First File " + lastFile);
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, " TelekomRecorder In ParseFileNameLocal() -->> Last File Is Null And There Is No File To Set");
                    }
                }
                else
                {
                    if (File.Exists(lastFile))
                    {
                        fs = new FileStream(lastFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        br = new BinaryReader(fs, enc);
                        br.BaseStream.Seek(Position, SeekOrigin.Begin);
                        FileInfo fi = new FileInfo(lastFile);
                        Int64 fileLength = fi.Length;
                        Char c = ' ';
                        while (!Environment.NewLine.Contains(c.ToString()) && (br.BaseStream.Position < fileLength))
                        {
                            c = br.ReadChar();

                            if (Environment.NewLine.Contains(c.ToString()) || br.BaseStream.Position == fileLength)
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  TelekomRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is " + br.BaseStream.Position);
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  TelekomRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : FileLength Is " + fileLength);
                            }
                        }

                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  TelekomRecorder In ParseFileNameLocal() -->> Position Is : " + br.BaseStream.Position.ToString());
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  TelekomRecorder In ParseFileNameLocal() -->> Length Is : " + br.BaseStream.Length.ToString());

                        if (br.BaseStream.Position == br.BaseStream.Length || br.BaseStream.Position == br.BaseStream.Length - 1)
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  TelekomRecorder In ParseFileNameLocal() -->> 111111111111111");

                            for (int i = 0; i < fullfileName.Length; i++)
                            {
                                if (Dir + fullfileName[i].ToString() == lastFile)
                                {
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  TelekomRecorder In ParseFileNameLocal() -->> 222222222");

                                    if (i + 1 == fullfileName.Length)
                                    {
                                        FileName = lastFile;
                                        lastFile = FileName;
                                        Log.Log(LogType.FILE, LogLevel.INFORM, " TelekomRecorder In ParseFileNameLocal() -->> There Is No New File and Waiting For New Record");
                                        break;
                                    }
                                    else
                                    {
                                        FileName = Dir + fullfileName[(i + 1)].ToString();
                                        lastFile = FileName;
                                        Log.Log(LogType.FILE, LogLevel.INFORM, " TelekomRecorder In ParseFileNameLocal() -->> Reading Of The File " + fullfileName[i] + " Finished And Continiu With New File " + fullfileName[i + 1]);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            FileName = lastFile;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  TelekomRecorder In ParseFileNameLocal() -->> Continiu reading the last file : " + FileName);
                        }
                    }
                    else
                    {
                        SetNextFile(fullfileName);
                    }
                }
            }
            else
                FileName = Dir;

            if (br != null && fs != null)
            {
                br.Close();
                fs.Close();
            }

            lastFile = FileName;

            Log.Log(LogType.FILE, LogLevel.DEBUG, "  TelekomRecorder In ParseFileNameLocal() -->> Filename is: " + FileName);
            Log.Log(LogType.FILE, LogLevel.DEBUG, "  TelekomRecorder In ParseFileNameLocal() -->> Exit The Function");
        }

        protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            dayChangeTimer.Stop();
            if (remoteHost == "")
            {
                String fileLast = FileName;
                Stop();
                ParseFileName();
                if (FileName != fileLast)
                {
                    Position = 0;
                    lastLine = "";
                    lastFile = FileName;
                    Log.Log(LogType.FILE, LogLevel.INFORM, "File changed, new file is, " + FileName);
                }
                base.Start();
            }
            dayChangeTimer.Start();
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

        private string[] SortFiles(ArrayList filenameList)
        {
            long[] permanentfileName = new long[filenameList.Count];
            string[] fullfileName = new string[filenameList.Count];

            try
            {
                for (int i = 0; i < filenameList.Count; i++)
                {
                    permanentfileName[i] = CreateParseFileName(filenameList[i].ToString());

                    fullfileName[i] = filenameList[i].ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "In SortFiles -->> " + ex.Message.ToString());
                Log.Log(LogType.FILE, LogLevel.ERROR, "In SortFiles -->> " + ex.StackTrace.ToString());
            }

            Array.Sort(permanentfileName, fullfileName);

            return fullfileName;
        }

        private void SetNextFile(String[] filenameList)
        {
            try
            {
                long lastfileDate = CreateParseFileName(Path.GetFileName(lastFile));

                long[] permanentfileName = new long[filenameList.Length];

                for (int i = 0; i < filenameList.Length; i++)
                {
                    permanentfileName[i] = CreateParseFileName(filenameList[i]);
                }

                int _index = 0;
                bool controlofexistnextfile = false;

                for (int i = 0; i < permanentfileName.Length; i++)
                {
                    if (permanentfileName[i] > lastfileDate)
                    {
                        _index = i;
                        controlofexistnextfile = true;
                        break;
                    }
                }

                if (controlofexistnextfile)
                {
                    FileName = Dir + filenameList[_index];
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  TelekomRecorder In SetNextFile() -->> There Is No New File; Waiting For a New File");
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "  TelekomRecorder In SetNextFile -->> " + ex.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "  TelekomRecorder In SetNextFile -->> " + ex.StackTrace);
            }
        }

        private long CreateParseFileName(string fileName)
        {
            long parsedfileName = 0;

            #region Eski Dosya Yapýsý
            //LogArchiveCF_1111_23-05-10_00.00.00_24-05-10_00.00.00_0.txt
            //try
            //{
            //    string[] fileName2 = new string[7];
            //    string[] fileName3 = new string[3];
            //    string[] fileName4 = new string[3];

            //    int templenghtoffilename = fileName.Length;
            //    int lenghtoffilename = templenghtoffilename - 4;
            //    fileName = fileName.Substring(0, lenghtoffilename);

            //    fileName2 = fileName.Split('_');

            //    fileName3 = fileName2[2].Split('-');
            //    string tempfileName3 = fileName3[2] + fileName3[1] + fileName3[0];

            //    fileName4 = fileName2[4].Split('-');
            //    string tempfileName4 = fileName4[2] + fileName4[1] + fileName4[0];

            //    parsedfileName = Convert.ToInt64(fileName2[1] + tempfileName3 + tempfileName4 + fileName2[6]);
            //}
            //catch (Exception ex)
            //{
            //    Log.Log(LogType.FILE, LogLevel.ERROR, "In CreateParseFileName -->> " + ex.Message.ToString());
            //    Log.Log(LogType.FILE, LogLevel.ERROR, "In CreateParseFileName -->> " + ex.StackTrace.ToString());
            //} 
            #endregion

            ////LogArchiveCF_1111_23-05-10_00.00.00_24-05-10_00.00.00_0.txt
            try
            {
                string[] fileName2 = new string[6];
                string fileName3 = "";
                string fileName4 = "";

                int templenghtoffilename = fileName.Length;
                int lenghtoffilename = templenghtoffilename - 4;
                fileName = fileName.Substring(0, lenghtoffilename);

                fileName2 = fileName.Split('_');

                fileName3 = fileName2[2];
                string tempfileName3 = fileName3;

                fileName4 = fileName2[3].Split('-')[1];
                string tempfileName4 = fileName4;

                parsedfileName = Convert.ToInt64(tempfileName3 + tempfileName4 + fileName2[5]);

            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "In CreateParseFileName -->> " + ex.Message.ToString());
                Log.Log(LogType.FILE, LogLevel.ERROR, "In CreateParseFileName -->> " + ex.StackTrace.ToString());
            }
            return parsedfileName;
        }
    }
}
