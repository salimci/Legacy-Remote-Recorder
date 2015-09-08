using System;
using System.IO;
using System.Text;
using CustomTools;

namespace RemoteRecorderTest
{
    class MockSecurityManagerRemoteRecorder : CustomServiceBase
    {
        public int Processed { get; set; }
        public string OutputFile { get; set; }
        public bool OutputEnabled { get; set; }

        public override void SetData(object obj)
        {
            PrintDataCount(++Processed);
            PrintData(obj);
        }

        public override void SetData(string Dal, string virtualhost, object obj)
        {
            PrintDataCount(++Processed);
            PrintData(obj);
        }

        private void PrintData(object obj)
        {
            if (!OutputEnabled || string.IsNullOrEmpty(OutputFile))
                return;

            if (obj != null)
            {
                using (var fs = new StreamWriter(OutputFile, true))
                {
                    var i = 0;
                    foreach (var f in obj.GetType().GetFields())
                    {
                        if (i++ > 0)
                            fs.Write('\t');
                        fs.Write(f.GetValue(obj));
                    }
                    fs.WriteLine();
                }
            }
        }

        private void PrintDataVerbose(object obj)
        {
            if (!OutputEnabled || string.IsNullOrEmpty(OutputFile))
                return;

            if (obj != null)
            {
                using (var fs = new StreamWriter(OutputFile, true))
                {
                    var sb = new StringBuilder();
                    var i = 0;
                    foreach (var f in obj.GetType().GetFields())
                    {
                        if (i++ > 0)
                            sb.Append(',');
                        sb.Append(f.Name).Append("=[").Append(f.GetValue(obj)).Append("]");
                    }
                    fs.WriteLine(sb);
                }
            }
        }

        void PrintDataCount(int Process)
        {
            Console.Write("\b\b\b\b\b\b\b\b\b\b{0:D10}", Processed);
        }

        public string LastFile { get; set; }
        public override void SetReg(int Identity, string LastPosition, string LastLine, string LastFile, string LastKeywords)
        {
            PrintFileInfo(LastFile, LastPosition);
        }

        public override void SetReg(int Identity, string LastPosition, string LastLine, string LastFile, string LastKeywords, string LastRecDate)
        {
            PrintFileInfo(LastFile, LastPosition);
        }

        void PrintFileInfo(string LastFile, string LastPosition)
        {
            if (this.LastFile != LastFile)
            {
                this.LastFile = LastFile;
                Processed = "0" != LastPosition ? 1 : 0;
                Console.WriteLine("\nCurrent File: {0}", LastFile);
            }

            PrintDataCount(Processed);
        }
    }
}
