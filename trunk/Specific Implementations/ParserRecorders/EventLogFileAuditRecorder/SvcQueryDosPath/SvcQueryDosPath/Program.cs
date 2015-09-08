using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Install;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;

namespace SvcQueryDosPath
{
    class Program
    {
        static string[] GetArgs(string[] args, int offset = 0)
        {
            if (args == null || offset >= args.Length)
                return null;
            var nArgs = new string[args.Length - offset];
            args.CopyTo(nArgs, offset);
            return nArgs;
        }

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0].ToLowerInvariant())
                {
                    case "install":
                        new AssemblyInstaller(Assembly.GetExecutingAssembly(), GetArgs(args, 1)).Install(new Hashtable());
                        return;
                    case "uninstall":
                        new AssemblyInstaller(Assembly.GetExecutingAssembly(), GetArgs(args, 1)).Uninstall(new Hashtable());
                        return;
                    case "test":
                        new SvcQueryDosPath().Run(args);
                        Console.WriteLine("Press Any Key to Terminate");
                        Console.ReadKey();
                        return;
                }
            }
            var svc = new ServiceBase[] { new SvcQueryDosPath() };
            ServiceBase.Run(svc);
        }
    }
}
