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
using GlassfishRecorderInstallerForm;
using DAL;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;

namespace GlassfishRecorderInstaller
{
    [RunInstaller(true)]
    public partial class DBInstaller : Installer
    {
        private DBForm dbf;
        private EventLogInstaller eventLogInstaller;

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

        ///dName=[DNAME] /dDName=[DDNAME] /dType=[DTYPE] /dHost=[DHOST] /dUser=[DUSER] /dPass=[DPASS] /aPass=[APASS] /aKey=[AKEY]
        protected override void OnBeforeInstall(System.Collections.IDictionary savedState)
        {
            base.OnBeforeInstall(savedState);

            //Debugger.Break();
            try
            {                                
                Database.Provider p = 0;
                
                Database.AddProviderToRegister(p, "temp", Context.Parameters["dHost"], Context.Parameters["dDName"], Context.Parameters["dUser"], Context.Parameters["dPass"]);
                try
                {
                    Database.Fast = false;
                    switch (p)
                    {
                        case Database.Provider.SQLServer:
                            Database.ExecuteNonQuery("temp", "SELECT * FROM SYSOBJECTS");
                            break;
                        default:
                            Database.ExecuteNonQuery("temp", "Show tables");
                            break;
                    }
                }
                catch
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Natek\\DAL", true);
                    key.DeleteSubKeyTree("temp");
                    dbf = new DBForm(this);
                    dbf.dbName = Context.Parameters["dName"];
                    DialogResult dr = dbf.ShowDialog();
                    if (dr == DialogResult.Cancel)
                    {
                        Rollback(savedState);
                        return;
                    }
                    else
                        return;
                }

                RegistryKey keyAdd = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Natek\\DAL", true);
                keyAdd.DeleteSubKeyTree("temp");
                
                Database.AddProviderToRegister(p, Context.Parameters["dName"], Context.Parameters["dHost"], Context.Parameters["dDName"], Context.Parameters["dUser"], Context.Parameters["dPass"]);
            }
            catch
            {
                try
                {
                    RegistryKey keyAdd = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Natek\\DAL", true);
                    keyAdd.DeleteSubKeyTree("temp");
                }
                catch
                {
                }
                Database.AddProviderToRegister(Database.Provider.SQLServer, "test", "127.0.0.1", "test", "test", "test");
                
            }
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