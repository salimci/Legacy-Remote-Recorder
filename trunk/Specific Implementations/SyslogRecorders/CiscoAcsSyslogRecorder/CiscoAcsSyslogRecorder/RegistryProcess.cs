using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;
using System.Net.Sockets;
using Log;
using LogMgr;
namespace CiscoACSRecorder
{
    class RegistryProcess
    {
        private int trc_level;
        private int syslog_port;
        private string err_log;
        private string protocol;
        private ProtocolType pro;

        public int TRC_LEVEL 
        {
            get 
            {
                return trc_level;
            }
        }

        public int SYSLOG_PORT
        {
            get
            {
                return syslog_port;
            }
        }

        public string ERR_LOG
        {
            get
            {
                return err_log;
            }
        }

        public string PROTOCOL 
        {
            get 
            {
                return protocol;    
            }
        }

        public ProtocolType PRO 
        {
            get 
            {
                return pro;
            }
        }

        public RegistryProcess() 
        {
            trc_level = 4;
            syslog_port = 514;
            err_log ="";
            protocol = "UDP";
        }

        public RegistryProcess(int trc_level, int syslog_port, string err_log, string protocol) 
        {
            this.trc_level = trc_level;
            this.syslog_port = syslog_port;
            this.err_log = err_log;
            this.protocol = protocol;
        }

        public bool get_regisryforAgent()
        {
            RegistryKey rk = null;
            try
            {
                
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\CiscoACSRecorder.log";
                syslog_port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoACSRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoACSRecorder").GetValue("Trace Level"));
                pro = ProtocolType.Udp;
                return true;
            }
            catch (Exception)
            {
                InitializeLogger.L.Log(LogType.FILE, LogLevel.ERROR, "An error accured while reading the registry");
                return false;
            }
            finally 
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }   

        public bool get_registryforRemoteRecorder(int Id, string location, int traceLevel) 
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\CiscoACSRecorder" + Id + ".log";
                rk.Close();
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }

            try
            {
                if (location.Length > 1)
                {
                    if (location.Contains(':'.ToString()))
                    {
                        protocol = location.Split(':')[0];
                        this.syslog_port = Convert.ToInt32(location.Split(':')[1]);
                        if (protocol.ToLower() == "tcp")
                            this.pro = ProtocolType.Tcp;
                        else
                            this.pro = ProtocolType.Udp;
                    }
                    else
                    {
                        protocol = location;
                        this.pro = ProtocolType.Udp;
                        this.syslog_port = 514;
                    }
                }
                else
                {
                    this.pro = ProtocolType.Udp;
                    this.syslog_port = 514;
                }                

                this.trc_level = traceLevel;
            }
            catch (Exception)
            {
                return false; 
               
            }
            return true;
        }
    }
}