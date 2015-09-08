//Name: LabrisAdministrativeRecorder
//Writer: Ali Yıldırım
//Date: 28/07/2010

using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using Parser;
using Log;
using CustomTools;
using SharpSSH.SharpSsh;
using System.Collections;

namespace Parser
{
    class LabrisAdministrativeRecorder: Parser
    {
        public LabrisAdministrativeRecorder()
        : base()
        {
            LogName = "LabrisAdministrativeRecorder";
            usingKeywords = false;
            lineLimit = 1000;
        }

        public LabrisAdministrativeRecorder(string fileName)
            : base(fileName)
        { }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() -->> Parsing Specific line: " + line);
            if (string.IsNullOrEmpty(line))
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Line is Null Or Empty");
                return true;
            }

            if (!dontSend)
            {
                //Jul 22 20:38:14 2010 sshd[8243]: Accepted password for natek from 192.168.20.6 port 51352 ssh2
                //Jul 23 06:03:12 2010 sshd[21751]: Failed password for natek from 192.168.20.6 port 58266 ssh2
                //Jul 23 06:00:16 2010 sshd[19155]: pam_unix(sshd:session): session closed for user natek
                //Jul 23 06:16:51 2010 sshd[2486]: pam_unix(sshd:session): session opened for user natek by (uid=0)
                //Jul 23 01:47:29 2010 sshd[30310]: pam_unix(sshd:auth): authentication failure; logname= uid=0 euid=0 tty=ssh ruser= rhost=192.168.20.6  user=natek
                //Jul 23 01:52:38 2010 sshd[6589]: PAM 6 more authentication failures; logname= uid=0 euid=0 tty=ssh ruser= rhost=192.168.20.6  user=natek
                //Jul 23 01:52:38 2010 sshd[6589]: PAM service(sshd) ignoring max retries; 7 > 3

                string[] strArray = line.Split(new char[] {' '},StringSplitOptions.RemoveEmptyEntries);
                try
                {
                    CustomBase.Rec rec = new CustomBase.Rec();

                        string gun = strArray[1];
                        string ay = strArray[0];
                        string yil = strArray[3];
                        //DateTime date = Convert.ToDateTime(gun + "/" + ay + "/" + yil);
                        //rec.Datetime = date.Day + "/" + date.Month + "/" + date.Year + " " + strArray[2];

                        string date = gun[1] + "/" + ay[0] + "/" + yil[3] + " " + strArray[2];
                        rec.Datetime = Convert.ToDateTime(date).ToString("yyyy/MM/dd HH:mm:ss");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "LabrisAdministrativeRecorder Datetime : " + rec.Datetime);

                        rec.CustomInt2 = ObjectToInt32(strArray[4].Substring(5, strArray[4].Length - 2 - 5), 0);

                        if (strArray[5] == "Accepted" || strArray[5] == "Failed")
                        { 
                            rec.EventCategory = strArray[5] + " " + strArray[6];
                            rec.CustomStr1 = strArray[8];
                            rec.CustomStr3 = strArray[10];
                            rec.CustomInt1 = ObjectToInt32(strArray[12],0);
                            rec.Description = strArray[13];
                        }
                        else if (strArray[5].StartsWith("pam_unix"))
                        {
                            rec.EventCategory = strArray[6] + " " + strArray[7].TrimEnd(';');
                            rec.Description = strArray[5].Substring(9, strArray[5].Length - 2 - 9);
                            rec.CustomStr1 = strArray[10];

                            if (rec.Description == "sshd:auth")
                            { 
                                rec.CustomStr1 = strArray[14].Split('=')[1];
                                rec.CustomStr3 = strArray[13].Split('=')[1];
                            }
                        }
                        else
                        {
                            rec.Description = "";
                            for(int i = 5;i<strArray.Length;i++)
                            {
                                rec.Description += " " + strArray[i];
                            }
                            rec.Description.Trim();
                        }

                        rec.CustomStr4 = remoteHost;

                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() -->> Setting Record Data");
                        SetRecordData(rec);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() -->> Finish Record Data");
                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Line : " + line);
                    return true;
                }
            }
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() -->> ParsingSpesific Ends");

            return true;
        }

        protected override void ParseFileNameRemote()
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " LabrisAdministrativeRecorder In ParseFileNameRemote() -->> Enter The Function ");

                string stdOut = "";
                string stdErr = "";
                String line = "";
                
                se = new SshExec(remoteHost, user);
                se.Password = password;

                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Home Directory | " + Dir);

                    se.Connect();
                    se.SetTimeout(Int32.MaxValue);
                    String command = "ls -lt " + Dir + " | grep administrative";
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " LabrisAdministrativeRecorder In ParseFileNameRemote() -->> SSH command : " + command);
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    se.Close();

                    StringReader sr = new StringReader(stdOut);
                    ArrayList arrFileNameList = new ArrayList();

                    while ((line = sr.ReadLine()) != null)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "LabrisAdministrativeRecorder In ParseFileNameRemote() -->> Dosya ismi okundu: " + line);
                        String[] arr = line.Split('.');
                        if (arr[0].StartsWith("administrative") == true)
                        {
                            arrFileNameList.Add(arr);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "LabrisAdministrativeRecorder In ParseFileNameRemote() -->> Okunan Dosya ismi arrayFileNameList'e atıldı. ");
                        }
                    }

                    String[] dFileNameList = SortFiles(arrFileNameList);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "LabrisAdministrativeRecorder In ParseFileNameRemote() -->> arrayFileNameList'e atılan dosya isimleri sıralandı. ");

                    if (!String.IsNullOrEmpty(lastFile))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "LabrisAdministrativeRecorder In ParseFileNameRemote() -->> LastFile is not null: " + lastFile);

                        bool bLastFileExist = false;

                        for (int i = 0; i < dFileNameList.Length; i++)
                        {
                            if ((base.Dir + dFileNameList[i].ToString()) == base.lastFile)
                            {
                                bLastFileExist = true;
                                break;
                            }
                        }

                        if (bLastFileExist)
                        {
                            if (IsLineHereRemote(dFileNameList) == false)
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " LabrisAdministrativeRecorder In ParseFileNameRemote() -->>Last line could not find any file : " + lastFile);
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " LabrisAdministrativeRecorder In ParseFileNameRemote() -->>Directorydeki tüm dosyalar alınmak isteniyor ise LastFile ve Position'ı sıfırlayınız. ");
                            }
                            else
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " LabrisAdministrativeRecorder In ParseFileNameRemote() -->> FileName ayarlandı. FileName: " + FileName);
                            }
                        }
                        else
                        {
                            FileName = Dir + dFileNameList[0].ToString();
                            Position = 0;
                            lastFile = FileName;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " | LastFile Silinmis , Dosya Bulunamadı  Yeni File : " + FileName);
                        }
                    }
                    else
                    {
                            if (dFileNameList.Length > 0)
                            {
                                FileName = Dir + dFileNameList[0].ToString();
                                lastFile = FileName;
                                Position = 0;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "LabrisAdministrativeRecorder In ParseFileNameRemote() -->> LastName Is Null and FileName Is Setted To : " + FileName);
                            }
                            else
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  LabrisAdministrativeRecorder In ParseFileNameRemote() -->> In The Log Location There Is No Log File");
                            }
                    }
                }
                else
                {
                    FileName = Dir;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  LabrisAdministrativeRecorder In ParseFileNameRemote() -->> Directory file olarak gösterildi.: " + FileName);
                }
            }
            catch (Exception exp)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "LabrisAdministrativeRecorder In ParseFileNameRemote() In Catch -->>" + exp.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "LabrisAdministrativeRecorder In ParseFileNameRemote() In Catch -->>" + exp.StackTrace);
                return;
            }

            Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In ParseFileNameRemote() -->>  Exit The Function");
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
            catch (Exception ex)
            {
                if (reg == null)
                {
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  LabrisAdministrativeRecorder In ParseFileNameRemote() Exception Message -->> " + ex.Message);
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  LabrisAdministrativeRecorder In ParseFileNameRemote() Exception StackTrace -->> " + ex.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  LabrisAdministrativeRecorder In ParseFileNameRemote() Exception Message -->> " + ex.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  LabrisAdministrativeRecorder In ParseFileNameRemote() Exception StackTrace -->> " + ex.StackTrace);
                }
            }
        }

        private string[] SortFiles(ArrayList arrFileNames)
        {
            UInt64[] dFileNumberList = new UInt64[arrFileNames.Count];
            String[] dFileNameList = new String[arrFileNames.Count];

            for (int i = 0; i < arrFileNames.Count; i++)
            {
                if (arrFileNames[i].ToString().Split('.').Length <= 3)
                {
                    dFileNumberList[0] = 0;
                    dFileNameList[0] = arrFileNames[i].ToString();
                    arrFileNames.RemoveAt(i);
                    break;
                }
            }
            
            for (int i = 1; i < arrFileNames.Count; i++)
            {
                dFileNumberList[i] = Convert.ToUInt64(arrFileNames[i].ToString().Split('.')[1]);
                dFileNameList[i] = arrFileNames[i].ToString();
            }

            Array.Sort(dFileNumberList, dFileNameList);
            return dFileNameList;
        }

        /// <summary>
        /// Okunacak olan dosyayı belirler.
        /// </summary>
        /// <param name="dFileNameList"> Elimizdeki dosyaların sıralı listesi.</param>
        /// <returns></returns>
        private bool IsLineHereRemote(string[] dFileNameList)
        {
            bool geriGidiyoruz = false;
            lineCount = 0;
            lineBulundu = false;

            Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In IsLineHere() -->>  Enter the funcktion.");
            bool result = CheckPositionInOldFiles(lastFile);
            Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In IsLineHere() -->>  lastfile kontrol ediliyor: " + lastFile);
            
            if(lastFile == dFileNameList[0])
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In IsLineHere() -->> lastLine Administrative: " + lastFile);
                  if(lineBulundu == true)
                  {
                      Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In IsLineHere() -->> Line bulundu. " + lastLine);
                      //lastfile Administrative ve lastLine burada bulundu.
                        if (lineCount >= 1)
                        {
                            FileName = lastFile;
                            Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In IsLineHere() -->>  Administrative'de hala okunacak satır var. FileName: " + lastFile);
                                //last file administrative, okunacak line var ve bu filedan okumaya devam ediyoruz.
                            return true;
                        } else
                        {
                            FileName = lastFile;
                            Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In IsLineHere() -->>  Administrative de okunan son satır dosyanın sonu. Bu dosyada bekliyoruz. FileName: " + lastFile);
                                //last file administrative, okunacak line kalmamış fakat bu fileda beklemeye devam ediyoruz.
                            return true;
                        }      
                  }
                  else
                  {
                      Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In IsLineHere() -->> Administartive'de Line bulunamadı. lastLine: " + lastLine);
                      //lastfile Administrative ve lastLine burada bulunamadı.
                         geriGidiyoruz = true;
                         Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In IsLineHere() -->> Administartive dosyasının ismi değişmiş olabilir. Geri gidiyoruz. ");
                      // Administrative dan geri doğru giderek en son kaydedilen line aranacak.

                        if(geriGidiyoruz)
                        {
                            //Geri giderek Son kalınan yeri ve dosyayı bulmaya çalışıyoruz. 
                            bool eskilerdeBuldu = false;
                            int i = 0;
                            string localLastFile = lastFile;

                            do
                            {
                                i++;
                                lineCount = 0;
                                lineBulundu = false;
                               
                                localLastFile = Dir + dFileNameList[dFileNameList.Length - i];
                                eskilerdeBuldu = CheckPositionInOldFiles(localLastFile);
                                Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In IsLineHere() -->> Eskilerde Arıyoruz. Aranan dosya: " + localLastFile);
                            
                            } while(!eskilerdeBuldu && i < dFileNameList.Length - 1);
                            
                            if (eskilerdeBuldu)
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In IsLineHere() -->> Eskilerde Arıyoruz.LastLine bulundu. Bulunan file: " + localLastFile);
                                //Last line bir dosyada bulundu.
                                if(lineCount >= 1)
                                {
                                    //bulunan dosyada okunacak satır hala var.
                                    FileName = localLastFile;
                                    lastFile = FileName;
                                    Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In IsLineHere() -->> Eskilerde Arıyoruz.LastLine bulundu. Bulunan file'da okunacak hala satır var.");
                                    return true;
                                }
                                else
                                {
                                    //bulunan dosyanın sonuna gelinmiş. Dosyayı değiştiriyoruz.
                                    FileName = Dir + dFileNameList[dFileNameList.Length - i + 1];
                                    lastFile = FileName;
                                    Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In IsLineHere() -->> Eskilerde Arıyoruz.LastLine bulundu. Bulunan file'da okunacak satır kalmamış. FileName: " + FileName);
                                    return true;
                                }
                            }
                            else
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In IsLineHere() -->> Eskilerde Arıyoruz.LastLine hiçbir dosyada bulunamadı.");
                                //LastLine eski dosyalarda bulunamadı. return false.
                                return false; 
                            }
                        }
                  }
            }
            else
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In isLineHereRemote() -->> LastFile eski dosyaların birisi. lastFile: " + lastFile);
                if (lineBulundu == true)
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In isLineHereRemote() -->> LastLine lastFileda bulundu. lastLine: " + lastLine);
                    if (lineCount >= 1)
                    {
                        FileName = lastFile;
                        Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In isLineHereRemote() -->> LastFile da hala okunacak satır var. FileName: " + FileName);
                        //lastFile dan okumaya devam
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In isLineHereRemote() -->> LastFile'ın sonuna gelindi. Okunacak başka satır yok. Dosya değiştirilecek.");
                        
                        for (int i = 1; i < dFileNameList.Length; i++)
                        {
                            if (Dir + dFileNameList[i].ToString() == lastFile)
                            {
                                if (i + 1 == dFileNameList.Length)
                                {
                                    //lastFile directorydeki son dosya. Bu yüzden File name Administartiv e atanacak.
                                    FileName = Dir + dFileNameList[0];
                                    Position = 0;
                                    lastFile = FileName;
                                    Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In isLineHereRemote() -->> LastFile directory'deki son dosya. Okucak başka dosya yok. FileName : " + FileName);
                                    return true;
                                }
                                else
                                {
                                    //lastFile directorydeki son dosya değil. Bu yüzden FileName sıradaki dosya oldu.
                                    FileName = Dir + dFileNameList[(i + 1)].ToString();
                                    Position = 0;
                                    lastFile = FileName;
                                    Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In isLineHereRemote() -->> LastFile directory'deki son dosya değil. Ondan sonra oluşturulmuş dosyalar var. FileName : " + FileName);
                                    return true;
                                }
                            }
                        }
                    }
                }
                else
                { 
                //eski dosyalardaki LastLine lastFile da bulunamadı. başka dosya var ise FileName sıradaki dosyaya atanacak. 
                    Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In isLineHereRemote() -->> Eski dosyalardaki lastLine LastFileda bulunamadı. LastFiledan sonra oluşturulmuş dosya var ise ona atanacak.");

                    for (int i = 1; i < dFileNameList.Length; i++)
                    {
                        if (Dir + dFileNameList[i].ToString() == lastFile)
                        {
                            if (i + 1 != dFileNameList.Length)
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In isLineHereRemote() -->> LastFile directory'deki son dosya değil.");
                                //okunan file'dan sonra yeni bir file oluşturulmuş. Filename bu file'a atandı.
                                FileName = Dir + dFileNameList[(i + 1)].ToString();
                                Position = 0;
                                lastFile = FileName;
                                Log.Log(LogType.FILE, LogLevel.INFORM, "LabrisAdministrativeRecorder In isLineHereRemote() -->> LastFile directory'deki son dosya değil. Ondan sonra oluşturulmuş dosyalar var. FileName : " + FileName);
                                return true;
                            }
                        }
                    }
                    return false;
                }
            }
        
            return false;
        }

        public bool lineBulundu = false;
        public int lineCount = 0;
        /// <summary>
        /// Aldığı file içerisinde lastLine'ı ve varsa okunacak line'ı bulur.
        /// </summary>
        /// <param name="lastFile"> Belirli Pozisyondaki LastLine'ı Aradığımız Dosya İsmi.</param>
        /// <returns></returns>
        private bool CheckPositionInOldFiles(string lastFile)
        {
            string stdOut = "";
            string stdErr = "";
            String commandRead;
            StringReader stReader;
            String line = "";

            if (readMethod == "nread")
            {
                commandRead = "nread" + " -n " + Position + "," + 2 + "p " + lastFile;
                Log.Log(LogType.FILE, LogLevel.DEBUG, " LabrisAdministrativeRecorder In CheckPosition() -->> commandRead For nread Is : " + commandRead);
                
                se.Connect();
                se.RunCommand(commandRead, ref stdOut, ref stdErr);
                se.Close();
                Log.Log(LogType.FILE, LogLevel.DEBUG, " LabrisAdministrativeRecorder In CheckPosition() -->> commandRead'den dönen strOut : " + stdOut);

                stReader = new StringReader(stdOut);

                //lastFile'dan line ve posizton okundu ve şimdi test ediliyor. 
                while ((line = stReader.ReadLine()) != null)
                {
                    if (lastLine == line)
                    {
                        lineBulundu = true;
                        continue;
                    }

                    if (line.StartsWith("~?`Position"))
                    {
                        continue;
                    }

                    lineCount++;
                }
            }
            else
            {
                commandRead = "sed" + " -n " + Position + "," + (Position + 1) + "p " + lastFile;
                Log.Log(LogType.FILE, LogLevel.DEBUG, " LabrisAdministrativeRecorder In CheckPosition() -->> commandRead For nread Is : " + commandRead);
                
                se.Connect();
                se.RunCommand(commandRead, ref stdOut, ref stdErr);
                se.Close();
                Log.Log(LogType.FILE, LogLevel.DEBUG, " LabrisAdministrativeRecorder In CheckPosition() -->> commandRead'den dönen strOut : " + stdOut);

                stReader = new StringReader(stdOut);

                while ((line = stReader.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                    if (line == lastLine)
                    {
                        lineBulundu = true;
                        continue;
                    }
                    lineCount++;
                }
            }

            if (lineBulundu == true)
                return true;
            else
                return false;
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
    }
}