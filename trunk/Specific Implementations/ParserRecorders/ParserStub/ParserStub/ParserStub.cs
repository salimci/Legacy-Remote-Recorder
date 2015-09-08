using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using CustomTools;
using Log;
using Microsoft.Win32;
using SharpSSH.SharpSsh;

namespace Parser
{
  public class ParserStub : CustomBase, IDisposable
  {

    protected String FileName;
    protected String Dir;
    protected CLogger Log;
    protected String LogName;
    protected Int64 Position;
    protected Int64 lastTempPosition;
    protected Int32 Id;
    protected String Virtualhost;
    protected String Dal;
    protected String home;
    protected LogLevel logLevel;
    protected String lastLine;
    protected String lastFile;
    protected Int64 startLineCheckCount;
    protected bool usingRegistry;
    protected bool usingKeywords;
    protected bool keywordsFound;
    protected bool checkLineMismatch;
    private FileSystemWatcher fsw;
    private bool started;
    private bool startFromEndOnLoss;
    private Int32 threadSleepTime;
    private Int32 maxReadLineCount;
    private String lastKeywords;
    private String LastRecDate;
    private bool parsing;
    private Mutex parseLock;
    private bool disposeCheck;
    private bool loadWatcher;
    private int zone;
    protected string tempCustomVar1;

    protected RegistryKey reg = null;
    protected DateTime Today;
    protected System.Timers.Timer dayChangeTimer;

    //Ssh and remote connection
    protected SshExec se;
    protected String remoteHost;
    protected String user;
    protected String password;
    protected System.Timers.Timer checkTimer;
    protected bool usingCheckTimer;
    protected System.Timers.Timer checkTimerSSH;
    protected Int32 lineLimit;
    protected String readMethod;

#if STAYCONNECTED
                protected bool stayConnected;
#endif

    //Temporary Storage
    protected CustomBase.Rec sRec;
    protected CustomBase.Rec sRecv;
    protected bool storage;
    protected Encoding enc;

    private string stubLine;

    public ParserStub()
    {
      started = true;
      usingRegistry = true;
      usingKeywords = true;
      keywordsFound = false;
      startFromEndOnLoss = false;
      checkLineMismatch = true;
      lineLimit = 100;
      startLineCheckCount = 10;
      parsing = false;
      parseLock = new Mutex();
      disposeCheck = false;
      loadWatcher = false;
      readMethod = "sed";
      enc = Encoding.Default;

#if STAYCONNECTED
            stayConnected = false;
#endif

      Id = 0;

      /*checkTimer = new System.Timers.Timer(60000);
      checkTimer.Enabled = false;
      checkTimer.Elapsed += new System.Timers.ElapsedEventHandler(checkTimer_Elapsed);
      usingCheckTimer = false;*/

      checkTimerSSH = new System.Timers.Timer(10);
      checkTimerSSH.Enabled = false;
      checkTimerSSH.Elapsed += new System.Timers.ElapsedEventHandler(checkTimerSSH_Elapsed);

      Log = new CLogger();
      Log.SetLogLevel(LogLevel.ERROR);
    }

    public ParserStub(String file)
    {

    }

    public string StubLine
    {
      get { return stubLine; }
      set
      {
        stubLine = value;
        Parse();
        checkTimerSSH.Enabled = true;
      }
    }

    private void checkTimerSSH_Elapsed(object sender, ElapsedEventArgs e)
    {
      try
      {
        checkTimerSSH.Stop();

        //checkTimerSSH.Start();
      }
      catch (Exception)
      {
        //log
      }
    }

    private void Parse()
    {
      bool noerr = ParseSpecific(stubLine, false);
    }

    public virtual bool ParseSpecific(string line, bool dontSend)
    {
      return true;
    }

    public Action<Rec> SetRecordData { get; set; }

    public void Dispose()
    {
      //this is a dummy
    }

    public virtual String GetLocation()
    {
      return "";
    }

    protected void GetRegistry()
    {
    }

    protected void ParseFileName()
    {
    }

    protected virtual void ParseFileNameRemote()
    {
      throw new Exception("You havent implemented ParseFileNameRemote() method, please implement it. If you added this please remove \"ParseFileNameRemote();\"");
    }

    protected virtual void ParseFileNameLocal()
    {
      throw new Exception("You havent implemented ParseFileNameLocal() method, please implement it. If you added this please remove \"base.ParseFileNameLocal();\"");
    }

    public virtual void GetFiles()
    {
      throw new Exception("You havent implemented GetFiles() method, please implement it. If you added this please remove \"base.GetFiles();\"");
    }

    public virtual String[] SpaceSplit(String line, bool useTabs)
    {
      List<String> lst = new List<String>();
      StringBuilder sb = new StringBuilder();
      bool space = false;
      foreach (Char c in line.ToCharArray())
      {
        if (c != ' ' && (!useTabs || c != '\t'))
        {
          if (space)
          {
            if (sb.ToString() != "")
            {
              lst.Add(sb.ToString());
              sb.Remove(0, sb.Length);
            }
            space = false;
          }
          sb.Append(c);
        }
        else if (!space)
        {
          space = true;
        }
      }

      if (sb.ToString() != "")
        lst.Add(sb.ToString());

      return lst.ToArray();
    }

    public virtual String[] SpaceSplit(String line, bool useTabs, Char ignoreChar)
    {
      List<String> lst = new List<String>();
      StringBuilder sb = new StringBuilder();
      bool space = false;
      bool ignore = false;
      foreach (Char c in line.ToCharArray())
      {
        if (c == ignoreChar)
          ignore = !ignore;
        if (c != ' ' && (!useTabs || c != '\t'))
        {
          if (space)
          {
            if (sb.ToString() != "")
            {
              lst.Add(sb.ToString());
              sb.Remove(0, sb.Length);
            }
            space = false;
          }
          sb.Append(c);
        }
        else if (!space)
        {
          if (ignore)
            sb.Append(c);
          else
            space = true;
        }
      }

      if (sb.ToString() != "")
        lst.Add(sb.ToString());

      return lst.ToArray();
    }

    public virtual String[] SpaceSplit(String line, bool useTabs, Char ignoreCharStart, Char ignoreCharEnd)
    {
      List<String> lst = new List<String>();
      StringBuilder sb = new StringBuilder();
      bool space = false;
      bool ignore = false;
      foreach (Char c in line.ToCharArray())
      {
        if (c == ignoreCharStart)
          ignore = true;
        else if (c == ignoreCharEnd)
          ignore = false;
        if (c != ' ' && (!useTabs || c != '\t'))
        {
          if (space)
          {
            if (sb.ToString() != "")
            {
              lst.Add(sb.ToString());
              sb.Remove(0, sb.Length);
            }
            space = false;
          }
          sb.Append(c);
        }
        else if (!space)
        {
          if (ignore)
            sb.Append(c);
          else
            space = true;
        }
      }

      if (sb.ToString() != "")
        lst.Add(sb.ToString());

      return lst.ToArray();
    }

    public virtual String[] SpaceSplit(String line, bool useTabs, Char ignoreChar, Char seperator, bool dummy)
    {
      List<String> lst = new List<String>();
      StringBuilder sb = new StringBuilder();
      bool space = false;
      bool ignore = false;
      foreach (Char c in line.ToCharArray())
      {
        if (c == ignoreChar)
          ignore = !ignore;
        if (c != seperator && (!useTabs || c != '\t'))
        {
          if (space)
          {
            if (sb.ToString() != "")
            {
              lst.Add(sb.ToString());
              sb.Remove(0, sb.Length);
            }
            space = false;
          }
          sb.Append(c);
        }
        else if (!space)
        {
          if (ignore)
            sb.Append(c);
          else
            space = true;
        }
      }

      if (sb.ToString() != "")
        lst.Add(sb.ToString());

      return lst.ToArray();
    }

    public virtual String[] SpaceSplit(String line, bool useTabs, Char ignoreChar, bool includeEmpty, Char seperator)
    {
      List<String> lst = new List<String>();
      StringBuilder sb = new StringBuilder();
      bool space = false;
      bool ignore = false;
      Char lastChar = '`';
      foreach (Char c in line.ToCharArray())
      {
        lastChar = c;
        if (c == ignoreChar)
          ignore = !ignore;
        if (c != seperator && (!useTabs || c != '\t'))
        {
          if (space)
          {
            if (includeEmpty || sb.ToString() != "")
            {
              lst.Add(sb.ToString());
              sb.Remove(0, sb.Length);
            }
            space = false;
          }
          sb.Append(c);
        }
        else if (!space)
        {
          if (ignore)
            sb.Append(c);
          else
            space = true;
        }
        else if (includeEmpty && space)
        {
          lst.Add(sb.ToString());
          sb.Remove(0, sb.Length);
        }
      }

      if (includeEmpty || sb.ToString() != "")
        lst.Add(sb.ToString());
      if (includeEmpty && lastChar == seperator)
        lst.Add("");

      return lst.ToArray();
    }
  }
}
