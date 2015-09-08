using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using CustomTools;
using RemoteRecorderTest.Enum;

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
            if (TestConfig.OutputMode == TestOutputMode.ToFile)
            {
                PrintData(obj);
            }
            else if (TestConfig.OutputMode == TestOutputMode.ToDb)
            {

                SqlQueries.InsertOutput((CustomBase.Rec)obj);
            }
            PrintDataCount(++Processed);
        }

        private void PrintData(object obj)
        {
            if (!OutputEnabled || string.IsNullOrEmpty(OutputFile))
                return;

            if (obj != null)
            {
                using (var fs = new StreamWriter(OutputFile, true))
                {
                    var begin = 0;
                    var end = Program.MaxCell;
                    var next = false;
                    var format = "{0,-" + Program.MaxCell + "}|";
                    do
                    {
                        next = false;
                        foreach (var f in obj.GetType().GetFields())
                        {
                            var v = f.GetValue(obj) == null ? string.Empty : f.GetValue(obj).ToString().Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "    ");
                            string s;
                            if (v.Length > end)
                            {
                                next = true;
                                s = v.Substring(begin, Program.MaxCell);
                            }
                            else
                                s = v.Length > begin ? v.Substring(begin, v.Length - begin) : string.Empty;
                            fs.Write(format, s);
                        }
                        fs.WriteLine();
                        begin = end;
                        end += Program.MaxCell;
                    } while (next);
                    Program.PrintLine(fs, "-", Program.MaxCell, obj.GetType().GetFields().Length);
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
            PrintReg(Identity, LastPosition, LastLine, LastFile, LastKeywords);
            PrintFileInfo(LastFile, LastPosition);
        }

        private void PrintReg(int Identity, string LastPosition, string LastLine, string LastFile, string LastKeywords)
        {
            if (!OutputEnabled || string.IsNullOrEmpty(OutputFile))
                return;
            using (var fs = new StreamWriter(OutputFile + ".reg", true))
            {
                fs.WriteLine("[{0}]\t[{1}]\t[{2}]\t[{3}]\t[{4}]", Identity, LastPosition, LastLine, LastFile, LastKeywords);
            }
        }

        public override void SetReg(int Identity, string LastPosition, string LastLine, string LastFile, string LastKeywords, string LastRecDate)
        {
            if (TestConfig.InputMode == TestInputMode.FromDb)
            {
                SqlQueries.SetReg(Identity, LastPosition, LastLine, LastFile, LastKeywords, LastRecDate);
            }
            PrintReg(Identity, LastPosition, LastLine, LastFile, LastKeywords);
            PrintFileInfo(LastFile, LastPosition);
        }

        void PrintFileInfo(string LastFile, string LastPosition)
        {
            if (this.LastFile != LastFile)
            {
                this.LastFile = LastFile;
                Processed = 0;
                Console.WriteLine("\nCurrent File: {0}", LastFile);
            }

            PrintDataCount(Processed);
        }
    }
}
