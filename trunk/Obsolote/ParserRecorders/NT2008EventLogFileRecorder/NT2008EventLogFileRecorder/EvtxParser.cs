using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Log;
using Microsoft.Win32;
using SharpSSH.SharpSsh;
using NT2008EventLogFileRecorder;

namespace NT2008EventLogFileRecorder
{
    public class EvtxParser : Parser.Parser
    {
        public bool IsFilefinished;

        public EvtxParser()
        {
        }

        public EvtxParser(String fileName)
            : base(fileName)
        {
        }

        public override void Parse()
        {
            try
            {
                if (!parseLock.WaitOne(0))
                {
                    parseLock.WaitOne();
                    try
                    {
                        throw new Exception("[!!! WARNING: SAFE MESSAGE !!!] : Parse already been processed by another thread while this call has made");
                    }
                    finally
                    {
                        parseLock.ReleaseMutex();
                    }
                }
                try
                {
                    this.parsing = true;
                    this.Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In Parse() -->> Entered The Function",
                                 new int[0]);
                    if (this.remoteHost != "")
                    {
                        this.Log.Log(LogType.FILE, LogLevel.DEBUG,
                                     "  Parser In Parse() -->> Remote Host is : " + this.remoteHost, new int[0]);
                        this.checkTimerSSH.Stop();
                        if (!this.ReadRemote())
                        {
                            this.Position = 0L;
                            this.lastLine = "";
                            this.ReadRemote();
                        }
                        this.checkTimerSSH.Start();
                    }
                    else if (!this.ReadLocal())
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Parse. Go to ReadLocal.");
                        this.Position = 0L;
                        this.lastLine = "";
                        this.ReadLocal();
                    }
                }
                finally
                {
                    this.parsing = false;
                    this.parseLock.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "Parse: " + ex.Message);
            }
        }

        //protected Boolean IsFileFinished(String file)
        //{
        //    return RecordFields.IsFilefinished;
        //}

        protected Mutex callable = new Mutex();
        protected bool ReadLocal()
        {
            if (!callable.WaitOne(0))
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, "Parser In ReadLocal -- CALLED MULTIPLE TIMES STILL IN USE");
                callable.WaitOne();
                try
                {
                    throw new Exception("Parse already been processed by another thread while this call has made");
                }
                finally
                {
                    callable.ReleaseMutex();
                }
            }
            try
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, "Parser In ReadLocal -- Started ReadLocal.");

                Log.Log(LogType.FILE, LogLevel.INFORM, "Parser In ReadLocal --  lastline: " + lastLine);
                Log.Log(LogType.FILE, LogLevel.INFORM, "Parser In ReadLocal --  lastfile: " + lastFile);
                Log.Log(LogType.FILE, LogLevel.INFORM, "Parser In ReadLocal --  position: " + Position);

                string eventLogLocation = "";

                EventLogReader logReader = null;
                EventBookmark bookmark = null; 
                eventLogLocation = FileName;
                try
                {
                    if (Position > 0)
                    {
                        logReader = new EventLogReader(new EventLogQuery(eventLogLocation, PathType.FilePath, "*[System/EventRecordID=" + Position + "]"));
                        EventRecord evIns = logReader.ReadEvent();
                        if (evIns != null)
                        {
                            bookmark = evIns.Bookmark;
                        }
                        logReader.Dispose();
                        logReader = null;
                    }

                   
                    EventLogQuery eventsQuery = new EventLogQuery(eventLogLocation, PathType.FilePath, "*");
                    try
                    {
                        bool dontSend = false;
                        if (bookmark == null)
                        {
                            logReader = new EventLogReader(eventsQuery);
                        }
                        else
                        {
                            logReader = new EventLogReader(eventsQuery, bookmark);
                        }
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ReadLocal, Call ParseSpecific.");
                        //DateTime next = DateTime.Now;
                        //int MAX_RECORD = lineLimit;
                        //int nextTimeOut = (int)Math.Round(60000.0 / MAX_RECORD);

                        //if (nextTimeOut == 0)
                        //{
                        //    nextTimeOut = 1;
                        //}
                        IsFilefinished = false;
                        lastLine = "-";
                        for (EventRecord eventInstance = logReader.ReadEvent();
                             null != eventInstance;
                             eventInstance = logReader.ReadEvent())
                        {
                            long? recordId = ((EventLogRecord)eventInstance).RecordId;
                            if (recordId == null)
                            {
                                continue;
                            }
                            //next = next.AddMilliseconds(nextTimeOut); //
                            ParseSpecific(logReader, eventInstance, dontSend, eventLogLocation);
                            Position = recordId.Value;
                            SetRegistry();
                            //int sleep = (int)next.Subtract(DateTime.Now).TotalMilliseconds;
                            //if (sleep > 0)
                            //{
                            //    try
                            //    {
                            //        //Thread.Sleep(sleep);
                            //    }
                            //    catch (Exception)
                            //    {

                            //    }
                            //}

                        }

                        IsFilefinished = true;


                        //try
                        //{
                        //    Log.Log(LogType.FILE, LogLevel.INFORM, " Parser in ReadLocal() -->> the file will be deleted " + eventLogLocation);
                        //    logReader.Dispose();
                        //    File.Delete(eventLogLocation);
                        //    Log.Log(LogType.FILE, LogLevel.INFORM, " Parser in ReadLocal() -->> " + eventLogLocation + " deleted.");
                        //}
                        //catch (Exception exception)
                        //{
                        //    Log.Log(LogType.FILE, LogLevel.INFORM, " Parser in ReadLocal() -->> An error occurred. " + exception.Message);
                        //}
                    }
                    finally
                    {
                        if (logReader != null)
                        {
                            logReader.Dispose();
                        }
                    }
                }
                catch (EventLogNotFoundException e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "EVTX Parser in ReadLocal ERROR." + e.Message);
                }


                Log.Log(LogType.FILE, LogLevel.INFORM, "Parser In ReadLocal End -- Started ReadLocal.");

                Log.Log(LogType.FILE, LogLevel.INFORM, "Parser In ReadLocal End --  lastline: " + lastLine);
                Log.Log(LogType.FILE, LogLevel.INFORM, "Parser In ReadLocal End --  lastfile: " + lastFile);
                Log.Log(LogType.FILE, LogLevel.INFORM, "Parser In ReadLocal End --  position: " + Position);


                return true;
            }
            finally
            {
                callable.ReleaseMutex();
            }
        }

        protected virtual Boolean IsFileFinished(String file)
        {

            return IsFilefinished;
            //int lineCount = 0;
            //String stdOut = "";
            //String stdErr = "";
            //String commandRead;
            //StringReader stReader;
            //String line = "";

            //RecordFields.currentFile = file;

            //if (string.IsNullOrEmpty(RecordFields.currentFile))
            //{
            //    RecordFields.totalLineCountinFile = CountLinesInFile(file);
            //}

            //try
            //{
            //    using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            //    {
            //        Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> reading local file! " + file);
            //        Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> Position : " + Position);

            //        using (StreamReader sr = new StreamReader(file))
            //        {
            //            while ((line = sr.ReadLine()) != null)
            //            {
            //                //MessageBox.Show(line);
            //            }

            //            if ((line == sr.ReadLine()) != null)
            //            {
            //                return true;
            //            }
            //            else
            //            {
            //                return false;
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Log.Log(LogType.FILE, LogLevel.ERROR, "IsFileFinished() -->> An error occurred is file : " + lastFile + "  : " + ex.ToString());
            //    Log.Log(LogType.FILE, LogLevel.ERROR, "IsFileFinished() -->> " + ex.StackTrace);
            //    return false;
            //}
        } // IsFileFinished

        // ReadLocal

        public EventRecord DisplayEventAndLogInformation(EventRecord eventInstance, string fileName, EventLogReader logReader)
        {
            //EventRecord eventInstance = null;
            //for (eventInstance = logReader.ReadEvent(); null != eventInstance; eventInstance = logReader.ReadEvent())
            //{
            //    #region Gerçek
            //    //_eventId = eventInstance.Id;
            //    //_eventType = eventInstance.TaskDisplayName;
            //    //_description = eventInstance.FormatDescription();
            //    //_eventCategory = eventInstance.LevelDisplayName;
            //    //_computerName = eventInstance.MachineName;
            //    //DateTime dtCreate = Convert.ToDateTime(eventInstance.TimeCreated);
            //    //_dateTimeCreate = dtCreate.ToString(DateFormat);
            //    //DateTime dtNow = DateTime.Now;
            //    //_dateTimeNow = dtNow.ToString(DateFormat);
            //    #endregion
            //}
            eventInstance = logReader.ReadEvent();
            return eventInstance;
        } // DisplayEventAndLogInformation




    }
}
