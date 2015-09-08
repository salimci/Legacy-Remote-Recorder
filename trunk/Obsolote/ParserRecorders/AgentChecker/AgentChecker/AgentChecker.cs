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
using DAL;
using System.ComponentModel;

namespace AgentChecker
{
    [RunInstaller(true)]
    public class AgentChecker : Installer
    {
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
    }
}
