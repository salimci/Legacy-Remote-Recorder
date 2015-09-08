using System;
using System.Collections.Generic;
using System.Text;

namespace Parser
{
    public class LinuxStringReader
    {
        String local;
        Int32 position;
        Char[] arr;

        public LinuxStringReader(String str)
        {
            local = str;
            arr = local.ToCharArray();
        }

        public String ReadLine()
        {
            StringBuilder sb = new StringBuilder();

            if (position >= arr.Length)
                return null;

            for (Int32 i = position; i < arr.Length; i++)
            {
                sb.Append(arr[i]);
                position++;
                if (arr[i] == '\n')
                {
                    return sb.ToString();
                }
            }

            return sb.ToString();
        }

        public void ClearPosition()
        {
            position = 0;
        }

        public override string ToString()
        {
            return local;
        }
    }
}
