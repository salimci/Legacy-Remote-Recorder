/*
 * Database Installer
 * Copyright (C) 2008 Erdoðan Kalemci <erdogan.kalemci@natek.com.tr>
 * You have no rights to distrubute, modify and use this code unless writer gives permission
*/

//uncomment this to delete dal on uninstall
//#define DELETE_DAL

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using DAL;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;

namespace NtEventlogRecorderInstaller
{
    [RunInstaller(true)]
    public partial class DBInstaller : Installer
    {
     //   private EventLogInstaller eventLogInstaller;

        public DBInstaller()
        {
            InitializeComponent();
            /*
            eventLogInstaller = new EventLogInstaller();
            eventLogInstaller.Source = "AgentService";

            eventLogInstaller.Log = "Application";
            Installers.Add(eventLogInstaller);
             */
        }

        protected override void OnBeforeInstall(System.Collections.IDictionary savedState)
        {
            /*RegistryKey reg = Registry.LocalMachine.OpenSubKey("Software\\NATEK\\Security Manager\\Agent");
            try
            {
                String home = (String)reg.GetValue("Home Directory");

                if (!File.Exists("bin\\Syscr.exe"))
                {
                    throw new Exception("Agent is not installed.");
                }
            }
            catch
            {
                throw new Exception("Agent is not installed.");
            }*/

            if (Context.Parameters.ContainsKey("pass"))
            {
                if (!Context.Parameters.ContainsKey("name"))
                    throw new Exception("Internal error, name not set on MSI Package");

                RegistryKey regIn = Registry.LocalMachine.OpenSubKey("Software\\NATEK\\Security Manager\\Recorder\\" + Context.Parameters["name"], true);
                try
                {
                    regIn.SetValue("Password", Encrypter.Eyncrypt("natek12pass", Context.Parameters["pass"]));
                }
                catch
                {
                    throw new Exception("Cannot Set Password");
                }
            }

            base.OnBeforeInstall(savedState);
        }
#if DELETE_DAL
        protected override void OnBeforeRollback(System.Collections.IDictionary savedState)
        {
            try
            {
                RegistryKey keyToDelete = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Natek\\AS");
                string keyS = keyToDelete.GetValue("DBName").ToString();
                RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Natek\\DAL", true);
                key.DeleteSubKeyTree(keyS);
            }
            catch
            {
            }

            base.OnBeforeRollback(savedState);
        }

        protected override void OnBeforeUninstall(System.Collections.IDictionary savedState)
        {
            try
            {
                RegistryKey keyToDelete = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Natek\\AS");
                string keyS = keyToDelete.GetValue("DBName").ToString();
                RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Natek\\DAL", true);
                key.DeleteSubKeyTree(keyS);
            }
            catch
            {
                Console.WriteLine();
            }

            base.OnBeforeUninstall(savedState);
        }
#endif
    }
}