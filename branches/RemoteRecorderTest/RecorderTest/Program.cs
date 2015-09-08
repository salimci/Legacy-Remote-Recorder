using System;
using System.Net.Sockets;
using LogMgr;

namespace RecorderTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var syslogInstance = new Syslog("172.16.91.172", 514, ProtocolType.Udp);

            Console.WriteLine("test");
        }
    }
}
