using System.Text;

namespace Natek.Helpers.Text
{
    public static class QuotedPrintable
    {
        public static readonly int[] HexMap = new int[] {
            -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
0,1,2,3,4,5,6,7,8,9,-1,-1,-1,-1,-1,-1,
-1,10,11,12,13,14,15,-1,-1,-1,-1,-1,-1,-1,-1,-1,
-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
-1,10,11,12,13,14,15,-1,-1,-1,-1,-1,-1,-1,-1,-1,
-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1};

        public static string Decode(Encoding encoding, string quotedPrintable)
        {
            var buffer = Encoding.ASCII.GetBytes(quotedPrintable);
            var tracker = 0;
            var state = 0;
            var pre = 0;
            var i = 0;
            for (; i < buffer.Length; i++)
            {
                switch (state)
                {
                    case 0:
                        if (buffer[i] == '=')
                            state = 1;
                        else
                            buffer[tracker++] = buffer[i];
                        break;
                    case 1:
                        if (HexMap[buffer[i]] >= 0)
                        {
                            pre = HexMap[buffer[i]];
                            state = 2;
                        }
                        else if (buffer[i] == '\r')
                            state = 3;
                        else if (buffer[i] == '\n')
                            state = 0;
                        else
                        {
                            while (state >= 0)
                                buffer[tracker++] = buffer[i - state--];
                            state = 0;
                        }
                        break;
                    case 2:
                        if (HexMap[buffer[i]] >= 0)
                            buffer[tracker++] = (byte)(pre * 16 + HexMap[buffer[i]]);
                        else
                        {
                            while (state >= 0)
                                buffer[tracker++] = buffer[i - state--];
                        }
                        state = 0;
                        break;
                    case 3:
                        if (buffer[i] != '\n')
                        {
                            while (--state > 0)
                                buffer[tracker++] = buffer[i - state];
                        }
                        state = 0;
                        break;
                }
            }

            if (state == 3)
                --state;
            while (state > 0)
                buffer[tracker++] = buffer[i - state--];
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            return encoding.GetString(buffer, 0, tracker);
        }
    }
}
