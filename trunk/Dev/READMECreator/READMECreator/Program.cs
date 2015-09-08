using System;
using System.IO;
using System.Text;

namespace READMECreator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var fullFileName = Console.ReadLine();
            var fileStream = new FileStream(fullFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (var binaryReader = new BinaryReader(fileStream, Encoding.Default))
            {
                char ch;
                int charCounter = 0;
                while ((ch = binaryReader.ReadChar()) != null)
                {
                    var stream = new FileStream("TEST_README.TXT", FileMode.Append, FileAccess.Write);
                    using (var binaryWriter = new BinaryWriter(stream, Encoding.Default))
                    {
                        binaryWriter.Write(ch);
                        charCounter++;
                        if (charCounter >= 50 && ch == ' ')
                        {
                            binaryWriter.Write('\r');
                            charCounter = 0;
                        }
                    }
                }
            }
        }
    }
}

