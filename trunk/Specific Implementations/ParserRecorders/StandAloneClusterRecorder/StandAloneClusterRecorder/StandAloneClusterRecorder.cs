using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using Parser;
using Log;
using CustomTools;
using System.Collections;
using System.Globalization;

namespace Parser
{
    public class StandAloneClusterRecorder : Parser
    {
        Dictionary<String, Int32> dictHash;

        public StandAloneClusterRecorder()
            : base()
        {
            LogName = "StandAloneClusterRecorder";
            usingKeywords = false;
            lineLimit = 50;
        }

        public override void Init()
        {
            GetFiles();
        }

        public StandAloneClusterRecorder(String fileName)
            : base(fileName)
        {

        }

        public override bool ParseSpecific(String line, bool dontSend)
        {

            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() | Parsing Starts");
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() | line : " + line);

            if (string.IsNullOrEmpty(line) == true)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() | Line Ýs Null or Empty");
                return true;
            }

            if (line.Contains("INTEGER"))
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() | line.Contains(INTEGER)");
                return true;
            }

            if (line.Contains("cdrRecordType"))
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() | line.Contains(cdrRecordType)");
                return true;
            }


            string sKeyWord = "\"cdrRecordType\",\"globalCallID_callManagerId\",\"globalCallID_callId\",\"origLegCallIdentifier\",\"dateTimeOrigination\",\"origNodeId\",\"origSpan\",\"origIpAddr\",\"callingPartyNumber\",\"callingPartyUnicodeLoginUserID\",\"origCause_location\",\"origCause_value\",\"origPrecedenceLevel\",\"origMediaTransportAddress_IP\",\"origMediaTransportAddress_Port\",\"origMediaCap_payloadCapability\",\"origMediaCap_maxFramesPerPacket\",\"origMediaCap_g723BitRate\",\"origVideoCap_Codec\",\"origVideoCap_Bandwidth\",\"origVideoCap_Resolution\",\"origVideoTransportAddress_IP\",\"origVideoTransportAddress_Port\",\"origRSVPAudioStat\",\"origRSVPVideoStat\",\"destLegIdentifier\",\"destNodeId\",\"destSpan\",\"destIpAddr\",\"originalCalledPartyNumber\",\"finalCalledPartyNumber\",\"finalCalledPartyUnicodeLoginUserID\",\"destCause_location\",\"destCause_value\",\"destPrecedenceLevel\",\"destMediaTransportAddress_IP\",\"destMediaTransportAddress_Port\",\"destMediaCap_payloadCapability\",\"destMediaCap_maxFramesPerPacket\",\"destMediaCap_g723BitRate\",\"destVideoCap_Codec\",\"destVideoCap_Bandwidth\",\"destVideoCap_Resolution\",\"destVideoTransportAddress_IP\",\"destVideoTransportAddress_Port\",\"destRSVPAudioStat\",\"destRSVPVideoStat\",\"dateTimeConnect\",\"dateTimeDisconnect\",\"lastRedirectDn\",\"pkid\",\"originalCalledPartyNumberPartition\",\"callingPartyNumberPartition\",\"finalCalledPartyNumberPartition\",\"lastRedirectDnPartition\",\"duration\",\"origDeviceName\",\"destDeviceName\",\"origCallTerminationOnBehalfOf\",\"destCallTerminationOnBehalfOf\",\"origCalledPartyRedirectOnBehalfOf\",\"lastRedirectRedirectOnBehalfOf\",\"origCalledPartyRedirectReason\",\"lastRedirectRedirectReason\",\"destConversationId\",\"globalCallId_ClusterID\",\"joinOnBehalfOf\",\"comment\",\"authCodeDescription\",\"authorizationLevel\",\"clientMatterCode\",\"origDTMFMethod\",\"destDTMFMethod\",\"callSecuredStatus\",\"origConversationId\",\"origMediaCap_Bandwidth\",\"destMediaCap_Bandwidth\",\"authorizationCodeValue\"";

            if (dictHash != null)
                dictHash.Clear();
            dictHash = new Dictionary<String, Int32>();

            String[] fields = sKeyWord.Split(',');

            Int32 count = 0;

            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() | Set Dictionary dictHash");
            foreach (String field in fields)
            {
                dictHash.Add(field.Trim('\"'), count);
                count++;
            }

            if (!dontSend)
            {
                String[] arr = line.Split(',');
                try
                {
                    Rec rec = new Rec();
                    rec.LogName = LogName;
                    rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    rec.CustomStr1 = arr[dictHash["finalCalledPartyNumberPartition"]].Trim('\"');
                    rec.CustomStr2 = arr[dictHash["lastRedirectDnPartition"]].Trim('\"');
                    rec.CustomInt1 = ObjectToInt32(arr[dictHash["duration"]].Trim('\"'), 0);
                    rec.CustomStr3 = arr[dictHash["origDeviceName"]].Trim('\"');
                    rec.CustomStr4 = arr[dictHash["destDeviceName"]].Trim('\"');
                    rec.CustomInt6 = ObjectToInt64(arr[dictHash["dateTimeOrigination"]].Trim('\"'), 0);
                    rec.CustomStr5 = arr[dictHash["callingPartyNumber"]].Trim('\"');
                    rec.CustomStr6 = arr[dictHash["originalCalledPartyNumber"]].Trim('\"');

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() | Setting Record Data");
                    SetRecordData(rec);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() | Finish Record Data");

                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "ParseSpecific() | Line : " + line);
                    return false;
                }

            }

            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() | ParsingEnds");

            return true;
        }

        protected override void ParseFileNameLocal()
        {

            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {

                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Searching for file in directory: " + Dir);
                ArrayList arrFileNames = new ArrayList();

                foreach (String file in Directory.GetFiles(Dir))
                {

                    string sFile = Path.GetFileName(file).ToString();
                    if (sFile.StartsWith("cdr_StandAloneCluster") == true)
                        arrFileNames.Add(sFile);

                }

                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Sorting file in directory: " + Dir);
                String[] dFileNameList = SortFileNameByFileNumber(arrFileNames);


                if (string.IsNullOrEmpty(lastFile) == false)
                {

                    if (File.Exists(lastFile) == true)
                    {

                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | lastFile is not null  : " + lastFile);

                        FileStream fs = new FileStream(lastFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        BinaryReader br = new BinaryReader(fs, enc);

                        br.BaseStream.Seek(Position, SeekOrigin.Begin);
                        
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Position is: " + br.BaseStream.Position.ToString());
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Length is: " + br.BaseStream.Length.ToString());
                        
                        if (br.BaseStream.Position == br.BaseStream.Length - 1 || br.BaseStream.Position == br.BaseStream.Length)
                        {

                            Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal() | Position is at the end of the file so changing the File");

                            for (int i = 0; i < dFileNameList.Length; i++)
                            {

                                if (Dir + dFileNameList[i].ToString() == lastFile)
                                {
                                    if (i + 1 == dFileNameList.Length)
                                    {
                                        FileName = lastFile;
                                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                                            "ParseFileNameLocal() | Yeni Dosya Yok Ayný Dosyaya Devam : " + FileName);
                                        break;
                                    }
                                    else
                                    {
                                        FileName = Dir + dFileNameList[(i + 1)].ToString();
                                        lastFile = FileName;
                                        Position = 0;
                                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                                            "ParseFileNameLocal() | Yeni Dosya  : " + FileName);
                                        break;

                                    }

                                }

                            }

                        }
                        else
                        {

                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal | Dosya Sonu Okunmadý Okuma Devam Ediyor");
                            FileName = lastFile;
                            Log.Log(LogType.FILE, LogLevel.DEBUG,
                                "ParseFileNameLocal() | FileName = LastFile " + FileName);

                        }

                        fs.Close();
                        br.Close();

                    }
                    else
                    {

                        SetBirSonrakiFile(dFileNameList, "ParseFileNameLocal()");
                    }

                }
                else
                {
                    if (dFileNameList.Length > 0)
                    {
                        FileName = Dir + dFileNameList[dFileNameList.Length - 1];
                        lastFile = FileName;
                        Position = 0;
                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                               "ParseFileNameLocal() | LastFile Ýs Null Ýlk FileName Set  : " + FileName);
                    }
                }
            }
            else
            {
                FileName = Dir;
                lastFile = FileName;
            }


        }

        private string[] SortFileNameByFileNumber(ArrayList arrFileNames)
        {
            UInt64[] dFileNumberList = new UInt64[arrFileNames.Count];
            String[] dFileNameList = new String[arrFileNames.Count];

            for (int i = 0; i < arrFileNames.Count; i++)
            {
                dFileNumberList[i] = Convert.ToUInt64(arrFileNames[i].ToString().Trim().Split('_')[3].Trim());
                dFileNameList[i] = arrFileNames[i].ToString();
            }

            Array.Sort(dFileNumberList, dFileNameList);
            return dFileNameList;
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

        /// <summary>
        /// LastFile'ýn Numarasýna Göre Bir Sonraki Dosyayý Set Eder
        /// </summary>
        /// <param name="dFileNameList"></param>
        private void SetBirSonrakiFile(String[] dFileNameList, string sFunction)
        {

            try
            {
                for (int i = 0; i < dFileNameList.Length; i++)
                {
                    UInt64 lFileNumber = Convert.ToUInt64(dFileNameList[i].ToString().Trim().Split('_')[3].Trim());
                    UInt64 lLastFileNumber = Convert.ToUInt64(Path.GetFileName(lastFile).ToString().Trim().Split('_')[3].Trim());

                    if (lFileNumber > lLastFileNumber)
                    {
                        FileName = Dir + dFileNameList[i].ToString();
                        Position = 0;
                        lastFile = FileName;

                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                            sFunction + " | LastFile Silinmis , Dosya Bulunamadý  Yeni File : " + FileName);

                        break;
                    }
                }
            }
            catch (Exception ex)
            {

                Log.Log(LogType.FILE, LogLevel.DEBUG,
                     "SetBirSonrakiFile() | " + ex.Message);

            }
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
        public override void Start()
        {
            //try
            //{

            //    String keywords = GetLastKeywords();
            //    String[] arr = keywords.Split(',');
            //    if (dictHash == null)
            //        dictHash = new Dictionary<String, Int32>();
            //    if (arr.Length > 2)
            //        dictHash.Clear();
            //    Int32 count = 0;
            //    foreach (String keyword in arr)
            //    {
            //        if (keyword == "")
            //            continue;
            //        dictHash.Add(keyword, count);
            //        count++;
            //    }

            //}
            //catch (Exception e)
            //{
            //    Log.Log(LogType.FILE, LogLevel.ERROR, "Cannot read keywords, but parsing will continue");
            //    Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
            //}
            base.Start();

        }

        private int ObjectToInt32(string sObject, int iReturn)
        {
            try
            {
                return Convert.ToInt32(sObject);

            }
            catch
            {
                return iReturn;
            }

        }
        private long ObjectToInt64(string sObject, long iReturn)
        {
            try
            {
                return Convert.ToInt64(sObject);
            }
            catch
            {
                return iReturn;
            }

        }





    }
}
