//
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Timers;
using CustomTools;
using Log;

namespace Parser
{
    public class GlassfishRecorder : Parser
    {
        Dictionary<String, Int32> dictHash;
        public GlassfishRecorder()
            : base()
        {
            LogName = "GlassfishRecorder";
        }
        public override void Init()
        {
            GetFiles();
        }

        public GlassfishRecorder(String fileName)
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
                String[] arr = line.Split('|');
                if (arr.Length > 7)
                    try
                    {
                        Rec r = new Rec();
                        StringBuilder date = new StringBuilder();
                        bool first = false;
                        String[] strArr = arr[1].Split('.')[0].Split('T');
                        date.Append(strArr[0]).Append(' ').Append(strArr[1]);

                        DateTime dt = DateTime.Parse(date.ToString());
                        r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;
                        r.EventCategory = arr[2];
                        r.ComputerName = arr[3];
                        r.CustomStr1 = arr[4];
                        r.CustomStr2 = arr[5];
                        r.Description = arr[6];
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
                else
                {                    
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Could not parse the line");
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Line : " + line);
                    return true;
                }

            }
            return true;
        }

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
                    Log.Log(LogType.FILE, LogLevel.INFORM, "File changed, new file is, " + FileName);
                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Day Change Timer File is: " + FileName);
            }
            dayChangeTimer.Enabled = true;
        }

        protected override void ParseFileNameLocal()
        {
            FileStream fs = null;
            BinaryReader br = null;
            Int64 currentPosition = Position;
            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                UInt64 count = 0;
                Log.Log(LogType.FILE, LogLevel.INFORM, "Searching in directory : " + Dir);

                FileName = lastFile;
                try
                {
                    if (FileName.Length < 1)
                    {
                        foreach (String file in Directory.GetFiles(Dir))
                        {
                            String fname = Path.GetFileName(file);
                            if (fname.StartsWith("server"))
                            {
                                String[] arr = fname.Split('_');
                                String[] arrIn = arr[1].Split('T');
                                String[] arrDate = arrIn[0].Split('-');
                                StringBuilder sb = new StringBuilder();
                                sb.Append(arrDate[0]).Append(arrDate[1]).Append(arrDate[2]);
                                UInt64 newCount = Convert.ToUInt64(sb.ToString());
                                if (count < newCount)
                                {
                                    count = newCount;
                                    FileName = file;
                                    Position = 0;
                                }
                            }
                        }
                        Log.Log(LogType.FILE, LogLevel.INFORM, "Filename is null so getting last File: " + FileName);
                        lastFile = FileName;
                        return;
                    }
                }
                catch(Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, e.Message);
                }
                fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                br = new BinaryReader(fs, enc);
                br.BaseStream.Seek(Position, SeekOrigin.Begin);

                Log.Log(LogType.FILE, LogLevel.INFORM, "Position is: " + br.BaseStream.Position.ToString());
                Log.Log(LogType.FILE, LogLevel.INFORM, "Length is: " + br.BaseStream.Length.ToString());

                if (br.BaseStream.Position == br.BaseStream.Length - 1)
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Position is at the end of the file so changing the File");

                    String[] arrold = FileName.Split('_');
                    String[] arrInold = arrold[3].Split('.');
                    UInt64 oldCount = Convert.ToUInt64(arrInold[0]);
                    foreach (String file in Directory.GetFiles(Dir))
                    {
                        String fname = Path.GetFileName(file);
                        if (fname.StartsWith("server"))
                        {
                            String[] arr = fname.Split('_');
                            String[] arrIn = arr[1].Split('T');
                            String[] arrDate = arrIn[0].Split('-');
                            StringBuilder sb = new StringBuilder();
                            sb.Append(arrDate[0]).Append(arrDate[1]).Append(arrDate[2]);
                            UInt64 newCount = Convert.ToUInt64(sb);
                            if (newCount > oldCount)
                            {
                                if (count == 0)
                                    count = newCount;
                                if (count >= newCount)
                                {
                                    count = newCount;
                                    FileName = file;
                                    Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal New file is set to: " + FileName);
                                }
                            }
                        }
                    }
                }
                else if (br.BaseStream.Position > br.BaseStream.Length - 1)
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Unexpected situation has occured : Last position is bigger than the File Length");
                    Position = 0;
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Position is not at the end of the file so no changing the File");
                }
            }
            else
                FileName = Dir;

            Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal Filename is: " + FileName);
            br.Close();
            fs.Close();
            lastFile = FileName;
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
    }
}
