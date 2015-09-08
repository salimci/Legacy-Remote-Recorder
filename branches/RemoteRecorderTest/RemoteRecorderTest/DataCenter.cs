using System;

namespace RemoteRecorderTest
{
    static class DataCenter
    {
        public static RecorderArgs GetValuesFromConsole()
        {
            var args = new RecorderArgs();
            SetValuesFromConsole(args);
            return args;
        }

        public static void SetValuesFromConsole(RecorderArgs pArgs)
        {
            Console.Write("Location:");
            pArgs.Location = Console.ReadLine();

            Console.Write("LastPosition:");
            pArgs.LastPosition = Console.ReadLine();

            Console.Write("LastFile:");
            pArgs.LastFile = Console.ReadLine();

            Console.Write("LastKeywords:");
            pArgs.LastKeywords = Console.ReadLine();

            Console.Write("From End On Loss (y/n): ");
            pArgs.FromEndOnLoss = Console.ReadLine() == "y";

            Console.Write("MaxLineToWait:");
            pArgs.MaxLineToWait = int.Parse(Console.ReadLine());

            Console.Write("User:");
            pArgs.User = Console.ReadLine();

            Console.Write("Password:");
            pArgs.Password = Console.ReadLine();

            Console.Write("RemoteHost:");
            pArgs.RemoteHost = Console.ReadLine();

            Console.Write("SleepTime:");
            pArgs.SleepTime = int.Parse(Console.ReadLine());

            Console.Write("Trace Level:");
            pArgs.TraceLevel = int.Parse(Console.ReadLine());

            Console.Write("CustomVar1 (string):");
            pArgs.CustomVar1 = Console.ReadLine();

            Console.Write("CustomVar2 (int):");
            pArgs.CustomVar2 = int.Parse(Console.ReadLine());

            Console.Write("VirtualHost:");
            pArgs.VirtualHost = Console.ReadLine();

            Console.Write("Dal:");
            pArgs.Dal = Console.ReadLine();

            Console.Write("Zone (Time Diff wrt Utc):");
            pArgs.TimeZone = int.Parse(Console.ReadLine());

            Console.Write("OutputLocation:");
            pArgs.OutputLocation = Console.ReadLine();
        }

        public static void SetValuesFromFile(RecorderArgs pArgs)
        {
            pArgs.Location = @"o:\tmp\palo";
            pArgs.LastLine = "";
            pArgs.LastPosition = "0";
            pArgs.LastFile = @"tse.txt";
            pArgs.LastKeywords = @"";
            pArgs.FromEndOnLoss = false;
            pArgs.MaxLineToWait = 1000000;
            pArgs.User = "root";
            pArgs.Password = "Password12345";
            pArgs.RemoteHost = "127.0.0.1";
            pArgs.SleepTime = 10000;
            pArgs.TraceLevel = 4;
            pArgs.MaxRecordSend = 1;
            pArgs.MaxRespondTime = 1000;
            pArgs.CustomVar1 = @"FP=tse.txt";
            pArgs.CustomVar2 = 0;
            pArgs.VirtualHost = "test";
            pArgs.Dal = "Natekdb";
            pArgs.TimeZone = 0;
            pArgs.OutputLocation = @"o:\tmp\palo";
            /*
            pArgs.Location = @"/tmp/log";
            pArgs.LastLine = "";
            pArgs.LastPosition = "0";
            pArgs.LastFile = "";
            //SR=^([0-9]+:[0-9]+:[0-9]+:[0-9]+):([0-9]+\\/[0-9]+\\/[0-9]+[\\s][0-9]+:[0-9]+:[0-9]+\\.[0-9]+)[\\s]([A-Za-z]+)[\\s]+(.*);DF=yyyy/MM/dd H:mm:ss;PO=Code,Datetime,Category,Description
            pArgs.LastKeywords = @"SR=<([a-zA-Z]+\\s*[\\d]+,\\s[\\d]+\\s[\\d]+:[\\d]+:[\\d]+\\s[\\w]+)\\s*[\\w]+>\\s*<([\\w]+)>\\s*<[\\w\\s]+>\\s*<([\\w\\S]+)>\\s*(.*);DF=MMM d, yyyy hh:mm:ss tt;PO=Datetime,Category,Code,Description";
            pArgs.FromEndOnLoss = false;
            pArgs.MaxLineToWait = 1000000;
            pArgs.User = "root";
            pArgs.Password = "Password12345";
            pArgs.RemoteHost = "172.16.91.144";
            pArgs.SleepTime = 10000;
            pArgs.TraceLevel = 4;
            pArgs.MaxRecordSend = 1;
            pArgs.MaxRespondTime = 1000;
            pArgs.CustomVar1 = @"FP=.*.out[0-9]*;";
            pArgs.CustomVar2 = 0;
            pArgs.VirtualHost = "test";
            pArgs.Dal = "Natekdb";
            pArgs.TimeZone = 0;
            pArgs.OutputLocation = @"T:\tmp\weblogic";
            */

            /*
            pArgs.Location = @"UDP:514";
            pArgs.LastLine = "";
            pArgs.LastPosition = "0";
            pArgs.LastFile = "";
            pArgs.LastKeywords = "";
            pArgs.FromEndOnLoss = false;
            pArgs.MaxLineToWait = 1000000;
            pArgs.User = "";
            pArgs.Password = "";
            pArgs.RemoteHost = "172.16.91.164";
            pArgs.SleepTime = 10000;
            pArgs.TraceLevel = 4;
            pArgs.MaxRecordSend = 1;
            pArgs.MaxRespondTime = 1000;
            pArgs.CustomVar1 = @"";
            pArgs.CustomVar2 = 0;
            pArgs.VirtualHost = "test";
            pArgs.Dal = "natekdb";
            pArgs.TimeZone = 0;
            pArgs.OutputLocation = @"";
            */
        }

        public static void SetValuesFromDb(RecorderArgs pArgs)
        {
            SqlQueries.SetRecorderArgsValues(pArgs);
        }

    }
}
