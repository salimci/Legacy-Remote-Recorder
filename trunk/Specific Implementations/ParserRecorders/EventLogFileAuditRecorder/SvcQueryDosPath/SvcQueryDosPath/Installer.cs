using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using Microsoft.Win32;


namespace SvcQueryDosPath
{
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {
        public Installer()
        {
            InitializeComponent();
            Installers.Add(
                new ServiceProcessInstaller
                    {
                        Account = ServiceAccount.LocalSystem
                    });
            Installers.Add(
                new ServiceInstaller
                    {
                        ServiceName = "QueryDosPath",
                        Description = "Provides path translation between device names and logical volumes via tcp",
                        StartType = ServiceStartMode.Automatic
                    });
        }
    }
}
