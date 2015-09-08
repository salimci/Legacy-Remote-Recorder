/*
 * File Check Install Component
 * Copyright (C) 2008 Erdoðan Kalemci <olligan@gmail.com>
 * You have no rights to distrubute, modify and use this code unless writer gives permission
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration.Install;
using Microsoft.Win32;
using System.IO;
using System.Windows.Forms;

namespace AgentChecker
{
    public class AgentChecker : Installer
    {
        protected override void OnBeforeInstall(System.Collections.IDictionary savedState)
        {            
            RegistryKey reg = Registry.LocalMachine.OpenSubKey("Software\\NATEK\\Security Manager\\Agent");
            try
            {
                String home = (String)reg.GetValue("Home Directory");
                //Context.Parameters["TARGETDIR"] = home;

                if (!File.Exists("WAudit.exe"))
                {
                    throw new Exception("Agent is not installed.");
                    //MessageBox.Show("You must Install Agent First");
                    //Rollback(savedState);
                }
            }
            catch
            {
                throw new Exception("Agent is not installed.");
                //MessageBox.Show("You must Install Agent First");
                //Rollback(savedState);
            }
            base.OnBeforeInstall(savedState);
        }
    }
}
